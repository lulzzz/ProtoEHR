using System;
using System.Collections.Generic;
using Microsoft.InformationProtection.Exceptions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using Orleans;
using Orleans.Runtime;

namespace ProtoEHR.Grains
{


    public class PatientGrain : Grain, IPatientGrain, IIncomingGrainCallFilter
    {
        public class State
        {
            public PatientInformation Info { get; set; }
            public HashSet<Guid> records { get; set; }

        }
        private readonly ILogger logger;
        private readonly IPersistentState<State> state;

        public PatientGrain(ILogger<PatientGrain> logger, [PersistentState("State")] IPersistentState<State> state)
        {
            this.logger = logger;
            this.state = state;
        }

        public Task Invoke(IIncomingGrainCallContext context)
        {
            var clearance = SecurityLevel.Public;
            var level = RequestContext.Get("Level");

            if(level != null) {
                clearance = (SecurityLevel) level;
            }

            var contectMethodName = context.InterfaceMethod.Name;

            var check1 = Utils.GrainFilter(contectMethodName, nameof(this.GetAllRecords), clearance, SecurityLevel.Restricted);
            var check2 = Utils.GrainFilter(contectMethodName, nameof(this.GetPatientInformation), clearance, SecurityLevel.Restricted);

            if(!(check1 && check2)) {
                throw new AccessDeniedException($"Issuficient security clearance {contectMethodName}!");
            } 

            return context.Invoke();
           
        }


        public override Task OnActivateAsync()
        {
            if (state.State.records == null)
            {
                state.State.records = new HashSet<Guid>();
            }

            return base.OnActivateAsync();
        }


        public async Task<bool> RegisterPatient(PatientInformation information) {

            if (state.State.Info != null) return false;
            
            state.State.Info = information;
            await state.WriteStateAsync();
            return true;
        }


        public async Task<bool> RegisterRecord(Guid itemKey)
        {
            if (state.State.Info == null ) return false;
            state.State.records.Add(itemKey);
            await state.WriteStateAsync();
            return true;
        }


        public Task<ImmutableArray<Guid>> GetAllRecords() =>
            Task.FromResult(ImmutableArray.CreateRange(state.State.records));

        public Task<SecurityLevel> GetSecurityLevel() => Task.FromResult(this.state.State.Info.Clearance);

        public Task<PatientInformation> GetPatientInformation() => Task.FromResult(state.State.Info);

        public async Task<Guid?> NewRecord(RecordItem recordItem)
        {
            if(this.state.State.Info == null) return null;

            var thisPatient = this.AsReference<IPatientGrain>();

            var created = false;
            var recordId = default(Guid);
            while (!created)
            {
                recordId = Guid.NewGuid();

                created = await this.GrainFactory.GetGrain<IRecordGrain>(recordId)
                                    .RegisterRecord(recordItem);

                state.State.records.Add(recordId);
                logger.LogInformation($"\n Record was created {recordId}");
                
            }
            await state.WriteStateAsync();
            return recordId;
        }

        public async Task<string> GetRecords(){
            var history = "";
            foreach(var recordId in ImmutableArray.CreateRange(state.State.records)) {
                var description = await this.GrainFactory.GetGrain<IRecordGrain>(recordId).GetDescription();
                var patient = await this.GrainFactory.GetGrain<IRecordGrain>(recordId).GetPatient();
                history += $"\n {description} {patient}";
            }
            return history;
        }



    }
}
