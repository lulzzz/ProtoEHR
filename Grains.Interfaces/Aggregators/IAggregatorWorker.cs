using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace ProtoEHR.Grains
{


    public interface IAggregatorWorker : IGrainWithIntegerKey
    {
        Task AddRecord(RecordItem recordItem);
    }
}