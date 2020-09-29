using System;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using System.Collections.Generic;
using Orleans.Runtime;

namespace ProtoEHR.Grains
{


    [StatelessWorker]
    public class AggregatorWorker : Grain, IAggregatorWorker
    {
         List<DataItem> items= new List<DataItem>();

        public override Task OnActivateAsync()
        {
            RegisterTimer(SendUpdate, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            return base.OnActivateAsync();
        }

        async Task SendUpdate(object _)
        {
            if (this.items.Count == 0) return;
            var totalScoreGrain = GrainFactory.GetGrain<ITotalAggregatorGrain>(Guid.Empty);
            await totalScoreGrain.AddItems(this.items);
            this.items = new List<DataItem>();
        }

        public async Task AddRecord(RecordItem recordItem)
        {
            RequestContext.Set("Level", SecurityLevel.Private);
            var patientInformation = await recordItem.Patient.GetPatientInformation();
            this.items.Add(new DataItem(patientInformation, recordItem));
        }
    }
}
