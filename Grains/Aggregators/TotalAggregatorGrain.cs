using System;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;
using System.Linq;

namespace ProtoEHR.Grains
{

    // Total Score Grain implementation
    public class TotalAggregatorGrain : Grain, ITotalAggregatorGrain
    {
        Dictionary<Guid, DataItem> items = new Dictionary<Guid, DataItem> ();

        private static double Noise(double S, double eps) {
            var scale = (S/eps);
            var L = new Laplace(0, scale);
            return L.Sample();
        }

        public Task AddItems(List<DataItem> items) {
            foreach(var item in items) {
                this.items[item.rItem.Key] = item;
            }
            return Task.CompletedTask;
        }

        public Task<double> GetTotalNumberOfTests()
        {
            var eps = Math.Log(3);
            var M = 3;
            var S = M;

            var testsPerPatient = new Dictionary<int, int>();  

            foreach(var item in items.Values) {
                var p = item.pInfo.Key;
                var c = testsPerPatient.GetValueOrDefault(p, 0);
                if (c < M)
                    testsPerPatient[p] = c+1;
            }
            var total = testsPerPatient.Values.Sum();

            var noise = Noise(S, eps);

            return Task.FromResult(total + noise);
        }


        public Task<double> GetTotalNumberOfTestsPerDay(DateTime date) {
            var eps = Math.Log(3);
            var M = 2;
            var S = M;

            var itemsForDay = items
                                .Where(x => x.Value.rItem.Timestamp.Date == date)
                                .Select(x => x.Value)
                                .ToList();
                                
            var testsPerPatient = new Dictionary<int, int>();  

            foreach(var item in itemsForDay) {
                var p = item.pInfo.Key;
                var c = testsPerPatient.GetValueOrDefault(p, 0);
                if (c < M)
                    testsPerPatient[p] = c+1;
            }
            var total = testsPerPatient.Values.Sum();

            var noise = Noise(S, eps);

            return Task.FromResult(total + noise);
        }

        public Task<double> getPositiveTests(){
            var value = 0;

            var patientsSeen = new HashSet<int>();

            foreach(var item in items.Values) {
                var result = item.rItem.Result;
                var patientKey = item.pInfo.Key;
                if(result != null) {
                    if(result.TestResult && !patientsSeen.Contains(patientKey)) {
                        value += 1;
                        patientsSeen.Add(patientKey);
                    } 
                }
            }

            var eps = Math.Log(3);
            var S = 1;
            var noise = Noise(S, eps);

            return Task.FromResult(value + noise);
        }

        public Task<(int, double)> GetMostTestedPostalCode() {
            var postalCodes = new Dictionary<int, double> ();
            var eps = Math.Log(3);
            var S = 1;

            // Aggregate counts
            foreach(var item in items.Values){
                var result = item.rItem.Result;
                var area = item.pInfo.PostalCode;
                if(result != null){
                    var currentCount = postalCodes.GetValueOrDefault(area, 0);
                    postalCodes[area] = currentCount + 1;
                }
            }

            // add noise to each count and select the max count
           var maxWithNoise = postalCodes
                        .Select(o => (o.Key, o.Value+Noise(S, eps)))
                        .OrderByDescending(x => x.Item2)
                        .FirstOrDefault();

            return Task.FromResult(maxWithNoise);
        }

        public Task<List<(AgeRange,double)>> GetAgeRangePositiveTests() {
            var eps = Math.Log(3);
            var S = 1;

            var range = Enumerable.Range(0,120).ToList();
            var ranges = Utils.Chunk(range, 20);
            var ageRanges = ranges
                                .Select(r => (new AgeRange(r.First(), r.Last()), 0.0))
                                .ToList();

            foreach(var item in items.Values) {
                if(item.rItem.Result != null && item.rItem.Result.TestResult) {
                    var age = (DateTime.Today.Year - item.pInfo.Birthday.Year);
                    var chunk = (int) Math.Floor(age/20.0);
                    var count = ageRanges[chunk].Item2;
                    ageRanges[chunk] = (ageRanges[chunk].Item1, count + 1);
                }
            }

            var noisyAgeRanges = ageRanges
                                    .Select(x => (x.Item1, x.Item2 + Noise(S, eps)))
                                    .ToList();

            return Task.FromResult(noisyAgeRanges);
        }

        public Task<int> GetTotalNoNoise()
        {
            return Task.FromResult(this.items.Count);
        }
    }
}
