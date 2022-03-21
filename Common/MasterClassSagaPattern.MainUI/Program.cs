using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.Routing;
using System.IO;
using MassTransit;
using MasterClassSagaPattern.Common;
using MasterClassSagaPattern.MainUI;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);
builder.Host.UseSerilog(ConfigureLogging);

var application = builder.Build();

ConfigureMiddleware(application, application.Environment);
ConfigureEndpoints(application);

application.Run();

static void ConfigureLogging(HostBuilderContext _, LoggerConfiguration loggerConfiguration)
{
    loggerConfiguration.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {SourceContext}]{NewLine}[{Level}]{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                       .MinimumLevel.Debug()
                       .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Error)
                       .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Error);
    Serilog.Debugging.SelfLog.Enable(Console.Error);
    Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
}

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddControllersWithViews();

    services.AddDatabase();

    services.AddMassTransit(configuration);
}

static void ConfigureMiddleware(IApplicationBuilder application, IWebHostEnvironment enviromnent)
{
    if (enviromnent.IsDevelopment())
    {
        application.UseDeveloperExceptionPage();
    }
    else
    {
        application.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        application.UseHsts();
    }
    application.UseHttpsRedirection();
    application.UseStaticFiles();

    application.UseRouting();

    application.UseAuthorization();
}

static void ConfigureEndpoints(IEndpointRouteBuilder application)
{
    application.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
    application.MapHealthChecks("/health");
}