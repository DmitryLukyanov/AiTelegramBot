using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using QuizBotPlugin;
using TelegramBot.Filters;
using TelegramBot.Plugins;

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

            QuizBotBootstrapper.ConfigureServices(builder);
        }

        public static void ConfigureHost(IHost host)
        {
            var kernel = host.Services.GetRequiredService<Kernel>();
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            kernel.Plugins.AddFromType<TimePlugin>();
            kernel.Plugins.AddFromType<CreatePollPlugin>(serviceProvider: host.Services);
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            kernel.FunctionInvocationFilters.Add(new LoggingFilter(host.Services.GetRequiredService<ILogger<LoggingFilter>>()));
            kernel.PromptRenderFilters.Add(new PromptFilter(host.Services.GetRequiredService<ILogger<PromptFilter>>()));
            kernel.AutoFunctionInvocationFilters.Add(new EarlyPluginChainTerminationFilter());
#pragma warning restore SKEXP0120 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            QuizBotBootstrapper.InitializeKernel(kernel, host.Services);
        }
    }
}
