using System;

namespace ProtoEHR.Grains
{
    public enum UserType {
        Researcher,
        Nurse,
        Doctor
    }
    public class UserInformation {
        public Guid Key {get;}
        public SecurityLevel Clearance { get; }
        public string Name { get; }
        public Gender Gender { get; }
        public UserType Type { get; }

        public UserInformation(Guid key, SecurityLevel clearance, string name, Gender gender, UserType type)
        {
            Key = key;
            Clearance = clearance;
            Name = Name;
            Gender = gender;
            Type = type;
        }
    }

}