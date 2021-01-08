using MassTransit.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

namespace MasterClassSagaPattern.Common
{
    public static class HostingHelper
    {
        public static void ConfigureDevOps(HostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            var env = hostingContext.HostingEnvironment;
            config.AddJsonFile("devops.json", optional: true, reloadOnChange: true).AddJsonFile($"devops.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
        }

        public static void ConfigureLogging(HostBuilderContext hostingContext, LoggerConfiguration loggerConfiguration)
        {
            ConfigureLogging(hostingContext.Configuration, loggerConfiguration);
        }

        private static void ConfigureLogging(IConfiguration configuration, LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration.ReadFrom.Configuration(configuration);
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            LogContext.ConfigureCurrentLogContext();
        }
    }
}
