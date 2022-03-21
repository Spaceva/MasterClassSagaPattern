using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MasterClassSagaPattern.Common;

public static class ServiceCollectionExtensions
{
    public static void ConfigureMassTransit<TNamespace>(this IServiceCollection services, string virtualHost, string queueName)
    {
        services.AddMassTransit(cfgMassTransit =>
        {
            cfgMassTransit.AddConsumersFromNamespaceContaining<TNamespace>();
            cfgMassTransit.UsingRabbitMq((registrationContext, cfgBus) =>
            {
                cfgBus.Host("localhost", virtualHost, cfgHost =>
                {
                    cfgHost.Username("saga-demo");
                    cfgHost.Password("saga-demo");
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
