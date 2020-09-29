using System;
using System.Threading.Tasks;
using Orleans.TestingHost;
using Xunit;
using ProtoEHR.Grains;
using Orleans.Runtime;

namespace Tests
{

    [Collection(ClusterCollection.Name)]
    public class UseCaseTests
    {
        private readonly TestCluster _cluster;
        public UseCaseTests(ClusterFixture fixture) =>
            _cluster = fixture?.Cluster ?? throw new ArgumentNullException(nameof(fixture));

        [Fact]
        public async Task Test_unauthorized_creation()
        {
            var patient = _cluster.GrainFactory.GetGrain<IPatientGrain>(42);
            
            var userInfo = new UserInformation(Guid.NewGuid(), SecurityLevel.Restricted, "Nurse",Gender.Female, UserType.Nurse);
            var user = _cluster.GrainFactory.GetGrain<IUserGrain>(userInfo.Key);


            var recordInfo = new RecordItem(Guid.NewGuid(), patient, user, SecurityLevel.Private, "lol", RecordType.VirusTest);
            var record = _cluster.GrainFactory.GetGrain<IRecordGrain>(recordInfo.Key);

            var created = await record.RegisterRecord(recordInfo);

            try{
                RequestContext.Set("Level", userInfo.Clearance);
                var info = await record.GetInfo();
            } catch(Exception e) {
                Assert.Equal("Issuficient security clearance GetInfo!", e.Message);
            }
        }
        public async Task Test_authorized_creation()
        {
            var patient = _cluster.GrainFactory.GetGrain<IPatientGrain>(42);
            
            var userInfo = new UserInformation(Guid.NewGuid(), SecurityLevel.Private, "Doctor", Gender.Female, UserType.Doctor);
            var user = _cluster.GrainFactory.GetGrain<IUserGrain>(userInfo.Key);


            var recordInfo = new RecordItem(Guid.NewGuid(), patient, user, SecurityLevel.Private, "lol", RecordType.VirusTest);
            var record = _cluster.GrainFactory.GetGrain<IRecordGrain>(recordInfo.Key);

            var created = await record.RegisterRecord(recordInfo);
            try{
                RequestContext.Set("Level", userInfo.Clearance);
                var info = await record.GetInfo();
                Assert.Equal("lol", info.Description);
            } catch(Exception e) {
                Assert.Null(e);
            }
        }

       
    }
}
