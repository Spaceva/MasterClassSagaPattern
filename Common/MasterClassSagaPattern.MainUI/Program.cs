using MassTransit.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

namespace MasterClassSagaPattern.MainUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>().UseSerilog(ConfigureLogging);
               });

        public static void ConfigureLogging(WebHostBuilderContext hostingContext, LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {SourceContext}]{NewLine}[{Level}]{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                               .MinimumLevel.Debug()
                               .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Error)
                               .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Error);
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            LogContext.ConfigureCurrentLogContext();
        }
    }
}
