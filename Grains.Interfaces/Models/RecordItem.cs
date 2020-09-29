using System;
using Orleans.Concurrency;

namespace ProtoEHR.Grains
{
    public enum SecurityLevel {
        Private,
        Restricted,
        Public
    }

    public enum RecordType {
        VirusTest,
        AntistofTest
    }

    [Immutable]
    public class RecordItemResult {
        public Guid RecordKey {get;}
        public IUserGrain Reporter {get; set;}
        public SecurityLevel SecurityLabel { get; set; }
        public DateTime Timestamp { get; set; }
        public bool TestResult {get; set;}

         public RecordItemResult(Guid recordKey, IUserGrain reporter, SecurityLevel securityLabel, bool testResult)
        {
            RecordKey = recordKey;
            Reporter = reporter;
            SecurityLabel = securityLabel;
            Timestamp = DateTime.UtcNow;
            TestResult = testResult;
        }
    }

    [Immutable]
    public class RecordItem {

        public Guid Key {get;}
        public IPatientGrain Patient { get;  }
        public IUserGrain Reporter {get;}
        public SecurityLevel SecurityLabel { get; }
        public string Description { get; }
        public RecordType Type { get; }
        public DateTime Timestamp { get; }
        public RecordItemResult Result { get;  }

        public RecordItem(Guid key, IPatientGrain patient, IUserGrain reporter, SecurityLevel securityLabel, string description, RecordType type)
            : this(key, patient, reporter, securityLabel, description, type, DateTime.UtcNow, null)
        {
        }

        protected RecordItem(Guid key, IPatientGrain patient, IUserGrain reporter, SecurityLevel securityLabel, string description, RecordType type, DateTime timestamp, RecordItemResult result)
        {
            Key = key;
            Patient = patient;
            Reporter = reporter;
            SecurityLabel = securityLabel;
            Description = description;
            Type = type;
            Timestamp = timestamp;
            Result = result;
        }

        public RecordItem WithResult(RecordItemResult result) =>
            new RecordItem(Key, Patient, Reporter, SecurityLabel, Description, Type, Timestamp, result);

    }

}