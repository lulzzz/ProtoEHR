using System;
using System.Threading.Tasks;
using Orleans;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ProtoEHR.Grains
{
    public interface IUserGrain : IGrainWithGuidKey
    {
        Task<bool> RegisterUser(UserInformation info);
        Task<List<IRecordGrain>> GetRecordsFromPatient(IPatientGrain patient);
        Task RequestRecord();
        Task QueryAggregator();
    }
}