using System;
using Microsoft.Extensions.Logging;
using Microsoft.InformationProtection.Exceptions;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace ProtoEHR.Grains
{


    public class RecordGrainNoFilter : Grain, IRecordGrainNoFilter
    {
        public class State
        {
            public RecordItem item { get; set; }
        }
        private readonly ILogger logger;
        private readonly IPersistentState<State> state;

        public RecordGrainNoFilter(ILogger<RecordGrain> logger, [PersistentState("State")] IPersistentState<State> state)
        {
            this.logger = logger;
            this.state = state;
        }
        private Guid GrainKey => this.GetPrimaryKey();


        public async Task<bool> RegisterRecord(RecordItem recordItem)
        {
            if (state.State.item != null) return false;
            
           // if (recordItem.SecurityLabel < await recordItem.Reporter.GetSecurityLevelAsync()) throw new AccessDeniedException($"No write down!");

            logger.LogInformation($"{recordItem}");

            state.State.item = recordItem;

            await recordItem.Patient.RegisterRecord(GrainKey);

            await GrainFactory.GetGrain<IAggregatorWorker>(0).AddRecord(recordItem);
            await state.WriteStateAsync();
            return true;
        }

        public async Task<bool> RegisterTestResult(RecordItemResult result) {
             logger.LogInformation($"{state.State.item.Result}");
            if(state.State.item.Result != null) return false;
            state.State.item = state.State.item.WithResult(result);

             // Notify aggregator, Notify Doctor, Notify patient
            await GrainFactory.GetGrain<IAggregatorWorker>(0).AddRecord(state.State.item);
           
            await state.WriteStateAsync();
            return true;
        }
        public Task<bool?> GetTestResult() {
            if(state.State.item.Result == null) return Task.FromResult((bool?) null);
            return Task.FromResult((bool?)state.State.item.Result.TestResult);
        }

        public Task<IPatientGrain> GetPatient() => Task.FromResult(state.State.item.Patient);
        public Task<RecordItem> GetInfo() => Task.FromResult(state.State.item);
        public Task<string> GetDescription() => Task.FromResult(this.state.State.item.Description);
    }
}
