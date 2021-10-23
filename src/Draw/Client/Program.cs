using Draw.Client.Services;
using Draw.Client.UnhandledExceptions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Draw.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton<IGameService, GameService>();
            builder.Services.AddSingleton<IToolboxService, ToolboxService>();
            builder.Services.AddSingleton<ILoggerService, LoggerService>();
            UnhandledExceptionProvider exceptionProvider = new UnhandledExceptionProvider();
            builder.Logging.AddProvider(exceptionProvider);

            WebAssemblyHost host = builder.Build();

            ILoggerService loggerService = host.Services.GetRequiredService<ILoggerService>();
            exceptionProvider.LoggerService = loggerService;

            await host.RunAsync();
        }
    }
}
