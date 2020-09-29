using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Orleans;

namespace ProtoEHR.Client
{
    public class ClientHostedService : IHostedService
    {
        private readonly IClusterClient _client;

        public ClientHostedService(IClusterClient client)
        {
            this._client = client;
        }
        public enum Tasks {
            nothing,
            task,
            benchmark,
            exit,
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {

            var benchmarks = new Benchmarks(_client);
            var service = new Service(_client);

            var command = Tasks.nothing;
            try
            {
                while (command != Tasks.exit)
                {
                    switch(command) {
                        case Tasks.benchmark:
                            var res = await Benchmark_cmds(benchmarks);
                            if(!res) 
                                command = Tasks.nothing;
                            break;
                        case Tasks.task:
                            var res2 = await run_cmds(service);
                            if(!res2) 
                                command = Tasks.nothing;
                            break;
                        case Tasks.nothing:
                            Console.WriteLine("\nWhat do you wanna do?\n1) Run\n2) Benchmark\n3) Exit\n");
                            string cmd = Console.ReadLine();
                            switch(cmd){
                                case "2":
                                    command = Tasks.benchmark;
                                    break;
                                case "1":
                                    command = Tasks.task;
                                    break;
                                case "3":
                                    command = Tasks.exit;
                                    break;
                                default:
                                    command = Tasks.nothing;
                                    break; 
                            }
                            break;
                        case Tasks.exit:
                            break;
                    }
                }
            }
            finally
            {
                Console.WriteLine("Exit (ctrl-c)!");
            }
        }
        
        public async Task<bool> Benchmark_cmds(Benchmarks benchmarks) {
            Console.WriteLine("\nWhat benchmarks do you wanna run?:");
            Console.WriteLine("1) Create patients and users:");
            Console.WriteLine("2) Create records:");
            Console.WriteLine("3) Run the aggregators");
            Console.WriteLine("4) Read all the records");
            Console.WriteLine("5) Benchmark 5");
            Console.WriteLine("6) Go back");

            string cmd = Console.ReadLine();
            switch(cmd){
                case "1":
                    var n = 5000;
                    await benchmarks.Run(n, n, n);
                    return true;
                case "2":
                    await benchmarks.TestRecords(5000);
                    return true;
                case "3":
                    await benchmarks.TestAggregator();
                    return true;
                case "4":
                    await benchmarks.TestReads();
                    return true;
                case "5":
                    Console.WriteLine("How many??: ");
                    string stringNumber = Console.ReadLine();
                    var n2 = int.Parse(stringNumber);
                    await benchmarks.TestRecords(n2);
                    return true;
                case "6":
                    return false;
                default:
                    return true;
            }
        }

        public async Task<bool> run_cmds(Service service) {
            Console.WriteLine("\nWhat do you wanna run?:");
            Console.WriteLine("1) Task 1:");
            Console.WriteLine("2) Task 2:");
            Console.WriteLine("3) Task 3");
            Console.WriteLine("4) Go back");

            string cmd = Console.ReadLine();
            switch(cmd){
                case "1":
                    await service.task1();
                    return true;
                case "2":
                    await service.task2();
                    return true;
                case "3":
                    await service.task2();
                    return true;
                case "4":
                    return false;
                default:
                    return true;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}