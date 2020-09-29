using System;

namespace ProtoEHR.Grains
{

    public enum Gender {
        Male,
        Female,
    }
    public class PatientInformation {
        public int Key {get;}
        public SecurityLevel Clearance { get; }
        public string Name { get; }
        public Gender Gender { get; }
        public int PostalCode { get; }
        public DateTime Birthday {get; }


        public PatientInformation(int key, SecurityLevel clearance, string name, Gender gender, int postalCode, DateTime birthday) {
            Key = key;
            Clearance = clearance;
            Name = name;
            Gender = gender;
            PostalCode = postalCode;
            Birthday = birthday;
        }

    }


}