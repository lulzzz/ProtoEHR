using System;
using System.Threading.Tasks;
using Orleans.TestingHost;
using Xunit;
using ProtoEHR.Grains;
using Orleans.Runtime;

namespace Tests
{

    [Collection(ClusterCollection.Name)]
    public class PatientGrainTest
    {
        private readonly TestCluster _cluster;
        public PatientGrainTest (ClusterFixture fixture) =>
            _cluster = fixture?.Cluster ?? throw new ArgumentNullException(nameof(fixture));

        [Fact]
        public async Task Test_non_reqistered_patients()
        {
            var patient = _cluster.GrainFactory.GetGrain<IPatientGrain>(1);
            var guid = Guid.NewGuid();
            var created = await patient.RegisterRecord(guid);

            Assert.False(created);
        }

        [Fact]
        public async Task Test_record()
        {
            var patient = _cluster.GrainFactory.GetGrain<IPatientGrain>(0);
            var user = _cluster.GrainFactory.GetGrain<IUserGrain>(Guid.NewGuid());
            var discription = "Test went okay";
            var guid = Guid.NewGuid();
            var record = _cluster.GrainFactory.GetGrain<IRecordGrain>(guid);

            var recordItem = new RecordItem(guid, patient, user, SecurityLevel.Private, discription, RecordType.VirusTest);

            await record.RegisterRecord(recordItem);
            var desc = await record.GetDescription();

            Assert.Equal(desc, discription);
        }

        [Fact]
        public async Task Creating_record_with_registered_patient()
        {
            var patient = _cluster.GrainFactory.GetGrain<IPatientGrain>(2);
            var user = _cluster.GrainFactory.GetGrain<IUserGrain>(Guid.NewGuid());

            var recordKey = Guid.NewGuid();
            var record = _cluster.GrainFactory.GetGrain<IRecordGrain>(recordKey);

            var patientInformation = new PatientInformation(2, SecurityLevel.Public, "Bob", Gender.Male, 1234, DateTime.Today);
            await patient.RegisterPatient(patientInformation);
            var recordInfo = new RecordItem(recordKey, patient, user, SecurityLevel.Private, "lol", RecordType.VirusTest);
            var created = await record.RegisterRecord(recordInfo);
            Assert.True(created);
        }

        [Fact]
        public async Task Test_No_read_up(){
            var patient = _cluster.GrainFactory.GetGrain<IPatientGrain>(2);
            var user = _cluster.GrainFactory.GetGrain<IUserGrain>(Guid.NewGuid());
            var guid = Guid.NewGuid();
            var record = _cluster.GrainFactory.GetGrain<IRecordGrain>(guid);

            var recordInfo = new RecordItem(guid, patient, user, SecurityLevel.Private, "lol", RecordType.VirusTest);
            var created = await record.RegisterRecord(recordInfo);
            try{
                RequestContext.Set("Level", SecurityLevel.Public);
                var info = await record.GetInfo();
                Assert.Equal("lol", info.Description);
            } catch(Exception e) {
                Assert.Equal("Issuficient security clearance GetInfo!", e.Message);
            }
        }
    }
}
