using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using System.Collections.Generic;

namespace ProtoEHR.Grains
{

    public class AgeRange {
        public int Start {get;}
        public int End {get;}

        public AgeRange(int start, int end) {
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return $"{Start}-{End}";
        }
    }

    public class DataItem {
        public PatientInformation pInfo {get; set;}
        public RecordItem rItem {get; set;}

        public DataItem(PatientInformation patientInformation, RecordItem recordItem) {
            pInfo = patientInformation;
            rItem = recordItem;
        }
    }
    public interface ITotalAggregatorGrain : IGrainWithGuidKey
    {
        Task AddItems(List<DataItem> items);
        // Queries
        Task<double> GetTotalNumberOfTests();
        Task<double> GetTotalNumberOfTestsPerDay(DateTime date);
        Task<int> GetTotalNoNoise();
        Task<(int, double)> GetMostTestedPostalCode();
        Task<List<(AgeRange,double)>> GetAgeRangePositiveTests();
        Task<double> getPositiveTests();
    }
}