using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

namespace MasterClassSagaPattern.Common;

public static class HostingHelper
{
    public static void ConfigureLogging(HostBuilderContext _, LoggerConfiguration loggerConfiguration)
    {
        ConfigureLogging(loggerConfiguration);
    }

    private static void ConfigureLogging(LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {SourceContext}]{NewLine}[{Level}]{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                           .MinimumLevel.Debug()
                           .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Error);
        Serilog.Debugging.SelfLog.Enable(Console.Error);
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
    }
}
