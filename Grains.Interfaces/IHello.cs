using System.Threading.Tasks;
using Orleans;

namespace ProtoEHR.Grains
{
    public interface IHello : IGrainWithIntegerKey
    {
        Task<string> SayHello(string greeting);
    }
}