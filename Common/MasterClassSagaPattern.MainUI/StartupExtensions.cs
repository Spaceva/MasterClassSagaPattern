using GreenPipes;
using MassTransit;
using MasterClassSagaPattern.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MasterClassSagaPattern.MainUI
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            services.AddDatabase<MainDbContext>("api");

            return services;
        }

        public static IServiceCollection AddMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(cfgMassTransit =>
            {
                cfgMassTransit.AddConsumer<PaymentCancelledConsumer>();
                cfgMassTransit.AddBus(registrationContext =>
                {
                    return Bus.Factory.CreateUsingRabbitMq(cfgBus =>
                    {
                        cfgBus.Host("localhost", configuration.GetValue<string>("BusVirtualHost"), cfgHost =>
                        {
                            cfgHost.Username("guest");
                            cfgHost.Password("guest");
                        });

                        cfgBus.ReceiveEndpoint("api", cfgEndpoint =>
                        {
                            cfgEndpoint.ConfigureConsumers(registrationContext);
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

            return services;
        }
    }
}
