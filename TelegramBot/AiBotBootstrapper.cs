using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.SemanticKernel;
using TelegramBot.Filters;

namespace TelegramBot
{
    internal static class AiBotbootStrapper
    {
        public static void Configure(IHostApplicationBuilder builder)
        {
            builder.Services.AddOpenTelemetry().UseAzureMonitor();
            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            });
            builder.Services.AddSingleton<MyCvPlugin>();
            builder.Services.AddSingleton<AiBotInitializer>();
        }

        public static void ConfigureHost(IHost host)
        {
            var kernel = host.Services.GetRequiredService<Kernel>();
            kernel.FunctionInvocationFilters.Add(new LoggingFilter(host.Services.GetRequiredService<ILogger<LoggingFilter>>()));
            kernel.PromptRenderFilters.Add(new PromptFilter(host.Services.GetRequiredService<ILogger<PromptFilter>>()));
            kernel.AutoFunctionInvocationFilters.Add(new EarlyPluginChainTerminationFilter());
            kernel.Plugins.AddFromType<MyCvPlugin>("MyCv", host.Services);
#pragma warning restore SKEXP0120 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        }
    }

    public class AiBotInitializer(MyCvPlugin myCvPlugin)
    {
        public async Task Initialize()
        {
            await myCvPlugin.InitializeMyCv();
        }
    }
}
