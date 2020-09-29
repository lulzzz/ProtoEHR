using System;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using Orleans;
using Orleans.Runtime;

namespace ProtoEHR.Grains
{

    public class UserGrain : Grain, IUserGrain
    {
        public class State
        {
            public UserInformation Information { get;set; }
        }
        private readonly ILogger logger;
        private readonly IPersistentState<State> state;

        public UserGrain(ILogger<RecordGrain> logger, [PersistentState("State")] IPersistentState<State> state)
        {
            this.logger = logger;
            this.state = state;
        }

        public async Task<bool> RegisterUser(UserInformation info){
            if (state.State.Information != null) return false;
            state.State.Information = info;
            await state.WriteStateAsync();
            return true;
        }

        public async Task<List<IRecordGrain>> GetRecordsFromPatient(IPatientGrain patient){
            var records = new List<IRecordGrain>();
            var recordGuids = await patient.GetAllRecords();

            foreach (var recordGuid in recordGuids)
            {
                records.Add(this.GrainFactory.GetGrain<IRecordGrain>(recordGuid));
            }
            return records;
        }

        public Task RequestRecord() {
            return Task.CompletedTask;
        }

        public Task QueryAggregator() {
            
            return Task.CompletedTask;
        }
    }
}
