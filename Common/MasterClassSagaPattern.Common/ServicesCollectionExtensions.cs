using GreenPipes;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MasterClassSagaPattern.Common
{
    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection StartBusOnBoot(this IServiceCollection services)
            => services.AddHostedService<BusControl>();

        public static void ConfigureMassTransit<TNamespace>(this IServiceCollection services, string virtualHost, string queueName)
        {
            services.AddMassTransit(cfgMassTransit =>
            {
                cfgMassTransit.AddConsumersFromNamespaceContaining<TNamespace>();
                cfgMassTransit.AddBus(registrationContext =>
                {
                    return Bus.Factory.CreateUsingRabbitMq(cfgBus =>
                    {
                        cfgBus.Host("spacevanas", virtualHost, cfgHost =>
                        {
                            cfgHost.Username("guest");
                            cfgHost.Password("guest");
                        });

                        cfgBus.ReceiveEndpoint(queueName, cfgEndpoint =>
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
        }

        public static IServiceCollection AddDatabase<TContext>(this IServiceCollection services, string dbName, bool enableSensitiveDataLogging = false)
            where TContext : DbContext
        {
            services.AddDbContext<TContext>(dbContextBuilder =>
            {
                dbContextBuilder.UseInMemoryDatabase(dbName);
                dbContextBuilder.EnableSensitiveDataLogging(enableSensitiveDataLogging);
                dbContextBuilder.EnableDetailedErrors();
            });

            return services;
        }
    }
}
