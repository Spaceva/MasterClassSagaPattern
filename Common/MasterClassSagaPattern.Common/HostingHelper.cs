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
        public static void ConfigureLogging(HostBuilderContext hostingContext, LoggerConfiguration loggerConfiguration)
        {
            ConfigureLogging(hostingContext.Configuration, loggerConfiguration);
        }

#pragma warning disable IDE0060 // Supprimer le paramètre inutilisé
        private static void ConfigureLogging(IConfiguration configuration, LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {SourceContext}]{NewLine}[{Level}]{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                               .MinimumLevel.Debug()
                               .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Error);
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            LogContext.ConfigureCurrentLogContext();
        }
#pragma warning restore IDE0060 // Supprimer le paramètre inutilisé
    }
}
