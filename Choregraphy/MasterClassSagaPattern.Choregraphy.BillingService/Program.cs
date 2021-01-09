﻿using MasterClassSagaPattern.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MasterClassSagaPattern.Choregraphy.BillingService
{
    public class Program
    {
        private const string PROGRAMNAME = Constants.Queues.BILLING;

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
            services.ConfigureMassTransit<Program>("choregraphy", PROGRAMNAME);

            services.AddDatabase<BillingDbContext>(PROGRAMNAME);
        }
    }
}
