using MassTransit.Context;
using MasterClassSagaPattern.Common;
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
               .ConfigureAppConfiguration(HostingHelper.ConfigureDevOps)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>().UseSerilog(ConfigureLogging);
               });


        public static void ConfigureLogging(WebHostBuilderContext hostingContext, LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            LogContext.ConfigureCurrentLogContext();
        }
    }
}
