
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;

namespace ProtoEHR.Client
{
    /// <summary>
    /// Orleans test silo client
    /// </summary>
    public class Program
    {
        public static Task Main(string[] args)
        {
            return new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ClusterClientHostedService>();
                    services.AddSingleton<IHostedService>(_ => _.GetService<ClusterClientHostedService>());
                    services.AddSingleton(_ => _.GetService<ClusterClientHostedService>().Client);

                    services.AddHostedService<ClientHostedService>();

                    services.Configure<ConsoleLifetimeOptions>(options =>
                    {
                        options.SuppressStatusMessages = true;
                    });
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                })
                .RunConsoleAsync();
        }
    }
}


