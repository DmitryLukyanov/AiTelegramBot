using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.SemanticKernel;
using MyCvPlugin;
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

            MyCvBootstrapper.ConfigureServices(builder);
        }

        public static void ConfigureHost(IHost host)
        {
            var kernel = host.Services.GetRequiredService<Kernel>();
            kernel.FunctionInvocationFilters.Add(new LoggingFilter(host.Services.GetRequiredService<ILogger<LoggingFilter>>()));
            kernel.PromptRenderFilters.Add(new PromptFilter(host.Services.GetRequiredService<ILogger<PromptFilter>>()));
            kernel.AutoFunctionInvocationFilters.Add(new EarlyPluginChainTerminationFilter());
#pragma warning restore SKEXP0120 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            MyCvBootstrapper.InitializeKernel(host);
        }
    }
}
