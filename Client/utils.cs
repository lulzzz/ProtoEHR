using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orleans;
using Orleans.Runtime;

using ProtoEHR.Grains;

namespace ProtoEHR.Client
{
    
    public static class Utils
    {
        
        public static DateTime RandomBirthday(Random random) {
            DateTime start = new DateTime(1920, 1, 1);
            int range = (DateTime.Today - start).Days;           
            return start.AddDays(random.Next(range));
        }



        public static PatientInformation genPatientInfo(int id, List<string> names, Random random) {
            var clearance = SecurityLevel.Public;
            var gender = (Gender) random.Next(0,2);
            var name = names[random.Next(0,names.Count)];
            var postalCode = random.Next(1,10);
            var birthday = RandomBirthday(random);

            return new PatientInformation(id, clearance, name, gender, postalCode, birthday);
        }

        public static UserInformation genUserInfo(Guid guid, List<string> names, Random random) {
                var userType = (UserType) random.Next(0,3);
                var gender = (Gender) random.Next(0,2);
                var name = names[random.Next(0,names.Count)];
                var clearance = SecurityLevel.Public;
                if(userType == UserType.Researcher) 
                    clearance = SecurityLevel.Restricted;
                
                if(userType == UserType.Doctor) 
                    clearance = SecurityLevel.Private;
                return new UserInformation(guid, clearance, name, gender, userType);
        }

        public static RecordItem genRecordItem(Guid guid, IPatientGrain patient, IUserGrain reporter, Random random) {
                    var type = (RecordType) random.Next(0,2);

                    var record =  new RecordItem(guid, patient, reporter, SecurityLevel.Private, "Random description", type);

                    // half of the records have results with them
                    if(random.Next(0,2) == 0) {
                        var RecordItemResult = genRecordItemResult(guid, reporter, random);
                        return record.WithResult(RecordItemResult);
                    } 

                    return record;
        }

        public static RecordItemResult genRecordItemResult(Guid recordKey, IUserGrain reporter, Random random) {
                    var result = (random.Next(0,3) == 0); // one third are positive
                    var recordResult = new RecordItemResult(recordKey, reporter, SecurityLevel.Private, result);
                    return recordResult;
        }

    }

}