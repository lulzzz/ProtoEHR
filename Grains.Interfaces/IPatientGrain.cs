using System;
using System.Threading.Tasks;
using Orleans;
using System.Collections.Immutable;

namespace ProtoEHR.Grains
{
    public interface IPatientGrain : IGrainWithIntegerKey
    {
        Task<bool> RegisterPatient(PatientInformation information);
        Task<bool> RegisterRecord(Guid recordKey);
        Task<ImmutableArray<Guid>> GetAllRecords();
        Task<PatientInformation> GetPatientInformation();
        Task<SecurityLevel> GetSecurityLevel();
    }
}