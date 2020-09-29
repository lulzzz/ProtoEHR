
using System.Collections.Generic;
using System.Linq;

namespace ProtoEHR.Grains
{
    public static class Utils
    {

        public static bool GrainFilter(string contextMethodName, string methodName, SecurityLevel clearance, SecurityLevel securityLevel)
        {
            if (string.Equals(contextMethodName, methodName))
            {
                if (clearance > securityLevel) return false;
            }
            return true;
        }
        public static List<List<T>> Chunk<T>(IEnumerable<T> data, int size)
        {
            return data
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / size)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
        }
    }
}
