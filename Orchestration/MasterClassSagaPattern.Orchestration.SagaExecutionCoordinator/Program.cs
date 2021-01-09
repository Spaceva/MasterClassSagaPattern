using GreenPipes;
using MassTransit;
using MassTransit.Saga;
using MasterClassSagaPattern.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace MasterClassSagaPattern.Orchestration.SagaExecutionCoordinator
{
    public class Program
    {
        private const string PROGRAMNAME = Constants.Queues.SAGACOORDINATOR;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServiceCollection)
                .UseSerilog(HostingHelper.ConfigureLogging);

        private static void ConfigureServiceCollection(HostBuilderContext hostingContext, IServiceCollection services)
        {
            services.AddMassTransit(cfgMassTransit =>
            {
                cfgMassTransit.AddSaga<OrderSaga>().InMemoryRepository();
                
                cfgMassTransit.AddBus(registrationContext =>
                {
                    return Bus.Factory.CreateUsingRabbitMq(cfgBus =>
                    {
                        cfgBus.Host("spacevanas", "orchestration", cfgHost =>
                        {
                            cfgHost.Username("guest");
                            cfgHost.Password("guest");
                        });

                        cfgBus.ReceiveEndpoint(PROGRAMNAME, cfgEndpoint =>
                        {
                            cfgEndpoint.Saga(new InMemorySagaRepository<OrderSaga>());
                            cfgEndpoint.UseMessageRetry(cfgRetry =>
                            {
                                cfgRetry.Interval(2, TimeSpan.FromSeconds(5));
                            });
                        });

                        cfgBus.UseInMemoryOutbox();
                    });
                });
            });

            services.StartBusOnBoot();
        }
    }
}
