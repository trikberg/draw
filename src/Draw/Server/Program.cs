using Draw.Server.Game;
using Draw.Server.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using System.Linq;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
try
{
    logger.Info("Application starting up.");

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    builder.Services.AddSignalR();
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();
    builder.Services.AddSingleton<Lobby>();
    builder.Services.AddResponseCompression(opts =>
    {
        opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "application/octet-stream" });
    });

    var app = builder.Build();

    app.UseResponseCompression();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error");
    }

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.UseRouting();

    app.MapRazorPages();
    app.MapControllers();
    app.MapHub<GameHub>(GameHub.HubUrl);
    app.MapHub<ClientLogHub>(ClientLogHub.HubUrl);
    app.MapFallbackToController("Index", "Index");

    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    logger.Info("Application shutting down.");
    LogManager.Shutdown();
}
