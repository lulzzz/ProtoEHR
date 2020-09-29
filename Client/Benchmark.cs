using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Orleans;
using Orleans.Runtime;

using ProtoEHR.Grains;

namespace ProtoEHR.Client
{
    
    public class Benchmarks
    {
        
        private readonly IClusterClient _client;
        private List<string> _names;

        private Random _random;

        private List<UserInformation> _users = new List<UserInformation>();

        private List<PatientInformation> _patients = new List<PatientInformation>();
        private List<RecordItem> _records = new List<RecordItem>();

        public Benchmarks(IClusterClient client) {

            this._client = client;

            this._names = File.ReadAllLines("Client/TestData/names.csv").ToList();

            this._random = new Random(42);
        }

        public async Task generatePatients(int number) {

            var tasks = new List<Task<bool>>();

            for (int i = 0; i < number; i++)
            {
                var patientInformation = Utils.genPatientInfo(i, _names, _random);
                
                var patient = _client.GetGrain<IPatientGrain>(i);
                
                var task = patient.RegisterPatient(patientInformation);
                tasks.Add(task);
                _patients.Add(patientInformation);
                
            }

            var results = await Task.WhenAll(tasks);
        }

        public async Task generateUsers(int number) {

            var tasks = new List<Task<bool>>();

            for (int i = 0; i < number; i++)
            {
                var userInfo = Utils.genUserInfo(Guid.NewGuid(), _names, _random);

                var task = _client.GetGrain<IUserGrain>(userInfo.Key).RegisterUser(userInfo);
                
                tasks.Add(task);
                this._users.Add(userInfo);
            }

            var results = await Task.WhenAll(tasks);
        }

        public async Task generateRecords(int number, List<UserInformation> users, List<PatientInformation> patients) {

            var tasks = new List<Task<bool>>();

            var docters = users.Where(u => u.Type == UserType.Doctor).ToList();

            for (int i = 0; i < number; i++) {

                var patient = patients[_random.Next(0,patients.Count)];
                var doctor = docters[_random.Next(0,docters.Count)];
                var iPatient = _client.GetGrain<IPatientGrain>(patient.Key);
                var iDoctor = _client.GetGrain<IUserGrain>(doctor.Key);

                var recordInfo = Utils.genRecordItem(Guid.NewGuid(), iPatient, iDoctor, _random);

                var task = _client.GetGrain<IRecordGrain>(recordInfo.Key).RegisterRecord(recordInfo);

                tasks.Add(task);
                _records.Add(recordInfo);
            }

            var results = await Task.WhenAll(tasks);

     
        }

        public async Task<List<RecordItem>> generateRecordsNoFilter(int number, List<UserInformation> users, List<PatientInformation> patients) {

            var recordItems = new List<RecordItem>();
            var tasks = new List<Task<bool>>();

            var docters = users.Where(u => u.Type == UserType.Doctor).ToList();

            for (int i = 0; i < number; i++) {

                var patient = patients[_random.Next(0,patients.Count)];
                var doctor = docters[_random.Next(0,docters.Count)];
                var iPatient = _client.GetGrain<IPatientGrain>(patient.Key);
                var iDoctor = _client.GetGrain<IUserGrain>(doctor.Key);

                var recordInfo = Utils.genRecordItem(Guid.NewGuid(), iPatient, iDoctor, _random);

                var task = _client.GetGrain<IRecordGrainNoFilter>(recordInfo.Key).RegisterRecord(recordInfo);

                tasks.Add(task);
                recordItems.Add(recordInfo);
            }

            var results = await Task.WhenAll(tasks);

            return recordItems;
        }
        public async Task<Result> TestRun(Task task, string name) {
            var stopwatch = Stopwatch.StartNew();
            await task;
            var result = new Result (stopwatch.Elapsed.TotalSeconds, name);
            return result;
        }


        public async Task TestAggregator() {
            var total_grian = _client.GetGrain<ITotalAggregatorGrain>(Guid.Empty);
            var testResults = new List<Result>();

            testResults.Add(await TestRun(total_grian.GetTotalNumberOfTests(), "Total Tests"));
            testResults.Add(await TestRun(total_grian.getPositiveTests(), "Total positive Tests"));
            testResults.Add(await TestRun(total_grian.GetMostTestedPostalCode(), "Most Tested"));
            testResults.Add(await TestRun(total_grian.GetAgeRangePositiveTests(), "Agerange postive tests"));

            foreach(var result in testResults) {
                Console.WriteLine("");
                pprint(result.Name, result.Elapsed);
            }
        }

        public async Task TestRecords(int nRecords) {
            Console.WriteLine(this._users.Count);
            Console.WriteLine(this._patients.Count);
            
            if(this._users.Count > 1 && this._patients.Count > 1) {
                var stopwatch = Stopwatch.StartNew();
                var recordsNoFilter = await generateRecordsNoFilter(nRecords, _users, _patients);
                var genRecordsNoFilter = stopwatch.Elapsed.TotalSeconds;
                stopwatch.Restart();

                await generateRecords(nRecords, _users, _patients);
                var genRecordsElapsed = stopwatch.Elapsed.TotalSeconds;
                stopwatch.Stop();
                pprint("records", genRecordsElapsed);
                pprint("records no filter", genRecordsNoFilter);
            }
        }

        public async Task TestReads(){
            var tasks = new List<Task<RecordItem>>();
            foreach(var record in this._records) {
                RequestContext.Set("Level", SecurityLevel.Private);
                var task = _client.GetGrain<IRecordGrain>(record.Key).GetInfo();
                tasks.Add(task);
            }
            var stopwatch = Stopwatch.StartNew();
            var res = await Task.WhenAll(tasks);
            var elapsed = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Stop();
            pprint("Read records", elapsed);
        }


        public async Task Run(int nPatients, int nUsers, int nRecords) {
            var stopwatch = Stopwatch.StartNew();

            await generatePatients(nPatients);
            var genPatientElapsed = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            await generateUsers(nUsers);
            var genUsersElapsed = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Stop();

            pprint("patients", genPatientElapsed);
            pprint("users", genUsersElapsed);
        }

        
        public class Result {
            public double Elapsed {get;}
            public string Name {get;}

            public Result(double elapsed, string name) {
                Elapsed = elapsed;
                Name = name;
            }
        }

        public static void pprint(string name, double number) {
            Console.WriteLine($"{name}, {number.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture)}");
        }

    }

}