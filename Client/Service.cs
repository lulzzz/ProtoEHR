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
    
    public class Service
    {
        
        private readonly IClusterClient _client;
        private List<string> _names;

        private Random _random;

        private List<(IUserGrain, UserType)> _users = new List<(IUserGrain, UserType)>();
        private List<IPatientGrain> _patients = new List<IPatientGrain>();
         private List<Guid> _records = new List<Guid>();

        public Service(IClusterClient client) {

            this._client = client;

            this._names = File.ReadAllLines("Client/TestData/names.csv").ToList();

            this._random = new Random(42);
        }

         public async Task GenData(){
           await GenUsers();
           await GenPatients();
           await GenRecords();
        }

        public static DateTime RandomBirthday(Random random) {
            DateTime start = new DateTime(1920, 1, 1);
            int range = (DateTime.Today - start).Days;           
            return start.AddDays(random.Next(range));
        }

        public async Task GenUsers(){
            var guids = File.ReadAllLines("Client/TestData/users.csv").ToList();
            var names = File.ReadAllLines("Client/TestData/names.csv").ToList();
            var random = new Random(42);
            foreach(var guid in guids) {
                var pGuid = Guid.Parse(guid);
                var userInfo = Utils.genUserInfo(pGuid, names, random);
                
                var user = _client.GetGrain<IUserGrain>(userInfo.Key);

                var res = await user.RegisterUser(userInfo);

                if (res) {
                   // Console.WriteLine($"User: {clearance}, {name}, {gender}, {userType}");
                    _users.Add((user, userInfo.Type));
                }
            }
            Console.WriteLine($"Generated {_users.Count} users");
        }

        public async Task GenPatients() {
            var random = new Random(42);
            var names = File.ReadAllLines("Client/TestData/names.csv").ToList();
            for (int i = 0; i < 100; i++) {
                var patientInformation = Utils.genPatientInfo(i, names, random);
                
                var patient = _client.GetGrain<IPatientGrain>(i);
                
                var res = await patient.RegisterPatient(patientInformation);
                if (res) {
                    //Console.WriteLine($"Patient: {clearance}, {name}, {gender}, {postalCode}, {birthday}");
                    _patients.Add(patient);
                }
            }
            Console.WriteLine($"Generated {_patients.Count} patients");
        }

        public async Task GenRecords() {
            var random = new Random(42);
            var guids = File.ReadAllLines("Client/TestData/records.csv").ToList();
            if(_users.Count > 0 && _patients.Count > 0) {
                
                var docters = _users.Where(u => u.Item2 == UserType.Doctor).Select(u => u.Item1).ToList();

                foreach(var guid in guids) {
                    var patient = _patients[random.Next(0,_patients.Count)];
                    var doctor = docters[random.Next(0,docters.Count)];
                    var type = (RecordType) random.Next(0,2);
                    var pGuid = Guid.Parse(guid);

                    var record = new RecordItem(pGuid, patient, doctor, SecurityLevel.Private, "desciprtion", type);
                    var res = await _client.GetGrain<IRecordGrain>(pGuid).RegisterRecord(record);
                    if(res){
                        _records.Add(pGuid);
                    //    Console.WriteLine($"Patient: {patient}, {doctor}, {type}, {pGuid}");

                        if(random.Next(0,2) == 0) {
                            var result = (random.Next(0,3) == 0);
                            await GenSingleRecordResult(pGuid, doctor, result);
                        }
                    }
                }
                Console.WriteLine($"Generated {_records.Count} records");
            }
        }

        public async Task GenSingleRecordResult(Guid key, IUserGrain reporter, bool result) {
            var recordResult = new RecordItemResult(key, reporter, SecurityLevel.Private, result);
            var res = await _client.GetGrain<IRecordGrain>(key).RegisterTestResult(recordResult);
        }

        public async Task task1(){
            await GenData();
            var patient = _client.GetGrain<IPatientGrain>(1234);
            var patientInformation = new PatientInformation(1234, SecurityLevel.Public, "Bob", Gender.Male,  1234, DateTime.Today.AddDays(-10));
            await patient.RegisterPatient(patientInformation);


            var record = new RecordItem(Guid.NewGuid(), patient, _users[0].Item1, SecurityLevel.Private, "Test went okay", RecordType.VirusTest);
            var recordResult = new RecordItemResult(record.Key, record.Reporter, SecurityLevel.Private, true);
            var recordNew = record.WithResult(recordResult);
            Console.WriteLine(recordNew.Result);
            var r = await _client.GetGrain<IRecordGrain>(record.Key).RegisterRecord(record);
            Console.WriteLine(r);
        }

        public async Task task2(){

            var total_grian = _client.GetGrain<ITotalAggregatorGrain>(Guid.Empty);
            var totalRecords = await total_grian.GetTotalNoNoise();
            var totalRecords2 = await total_grian.GetTotalNumberOfTests();

            var totalRecordsPerday = await total_grian.GetTotalNumberOfTestsPerDay(DateTime.Today.AddDays(-10));
            var positive = await total_grian.getPositiveTests();
            var mostTested = await total_grian.GetMostTestedPostalCode();
            var ageRanges = await total_grian.GetAgeRangePositiveTests();
    
            Console.WriteLine($"Total records (noNoise): {totalRecords}");
            Console.WriteLine($"Total records: {totalRecords2}");
            Console.WriteLine($"Total records 10 days ago: {totalRecordsPerday}");
            Console.WriteLine($"Total Positve records: {positive}");
            Console.WriteLine($"Most records postal Code {mostTested.Item1} with {mostTested.Item2} records taken");

            foreach(var range in ageRanges) {
                Console.WriteLine($"Age-range {range.Item1.ToString()} has {range.Item2} positive records");
            }
        }

        public async Task Task3(){
            var user = _client.GetGrain<IUserGrain>(Guid.NewGuid());
            var patient = _client.GetGrain<IPatientGrain>(0);

            var records = await user.GetRecordsFromPatient(patient);

            foreach(var record in records) {

                RequestContext.Set("Level", SecurityLevel.Public);
                try
                {
                    var info = await record.GetInfo();

                    Console.WriteLine($"{info.Description}");
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
        }


    }

}