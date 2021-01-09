using GreenPipes;
using MassTransit;
using MasterClassSagaPattern.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace MasterClassSagaPattern.StateMachine.SagaExecutionCoordinator
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
                cfgMassTransit.AddSagaStateMachine<OrderStateMachine, OrderState>().InMemoryRepository();

                cfgMassTransit.AddBus(registrationContext =>
                {
                    return Bus.Factory.CreateUsingRabbitMq(cfgBus =>
                    {
                        cfgBus.Host("spacevanas", "statemachine", cfgHost =>
                        {
                            cfgHost.Username("guest");
                            cfgHost.Password("guest");
                        });

                        cfgBus.ReceiveEndpoint(PROGRAMNAME, cfgEndpoint =>
                        {
                            cfgEndpoint.StateMachineSaga(registrationContext.GetService<OrderStateMachine>(), registrationContext);
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
