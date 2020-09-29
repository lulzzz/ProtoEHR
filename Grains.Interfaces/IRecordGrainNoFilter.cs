using System.Threading.Tasks;
using Orleans;
using ProtoEHR.Grains;

namespace ProtoEHR.Grains
{

    public interface IRecordGrainNoFilter : IGrainWithGuidKey
    {

        Task<bool> RegisterRecord(RecordItem recordItem);
        Task<bool> RegisterTestResult(RecordItemResult result);
        Task<bool?> GetTestResult();
        Task<IPatientGrain> GetPatient();
        Task<string> GetDescription();
        Task<RecordItem> GetInfo();
    }
}