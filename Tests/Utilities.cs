using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.TestingHost;
using Xunit;

using ProtoEHR;

namespace Tests
{
    public sealed class ClusterFixture : IDisposable
    {
        public ClusterFixture()
        {
            var builder =
                new TestClusterBuilder()
                    .ConfigureHostConfiguration(configurationBuilder =>
                        configurationBuilder.AddEnvironmentVariables());

            builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();

            Cluster = builder.Build();
            Cluster.Deploy();
        }

        public void Dispose()
        {
            Cluster.StopAllSilos();
        }

        public TestCluster Cluster { get; private set; }
    }

    public class TestSiloConfigurations : ISiloConfigurator
    {
        public IConfiguration Configuration { get; private set; }

        public void Configure(ISiloBuilder hostBuilder)
        {
            Configuration = hostBuilder.GetConfiguration();

            hostBuilder
                .AddMemoryGrainStorageAsDefault()
                .UseLocalhostClustering()
                .ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory());
        }
    }

    [CollectionDefinition(Name)]
    public class ClusterCollection : ICollectionFixture<ClusterFixture>
    {
        public const string Name = nameof(ClusterCollection);
    }
}