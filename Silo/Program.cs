using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using ProtoEHR.Grains;

namespace ProtoEHR.Silo
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            Console.Title = nameof(Silo);

            return new HostBuilder()
                .UseOrleans(builder =>
                {
                    builder
                        .UseLocalhostClustering()
                        .ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory()) 
                        .UseDashboard(options => { })
                        .AddMemoryGrainStorageAsDefault()
                        .AddMemoryGrainStorage("PubSubStore");
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                })
                .RunConsoleAsync();
        }
    }
}