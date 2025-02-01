using AiConnector.OpenAi.SemanticKernel;
using Microsoft.KernelMemory.SemanticKernel;
using Microsoft.SemanticKernel;

namespace TelegramBot
{
    internal static class AiBotbootStrapper
    {
        public static void Configure(HostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<MyCvPlugin>();
            builder.Services.AddSingleton<AiBotInitializer>();
        }

        public static void ConfigureHost(IHost host)
        {
            var kernel = host.Services.GetRequiredKeyedService<Kernel>(OpenAiModelSettings.ConfigurationKey);
#pragma warning disable SKEXP0120 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            kernel.ImportPluginFromObject<MyCvPlugin>(
                target: host.Services.GetRequiredService<MyCvPlugin>(),
                new System.Text.Json.JsonSerializerOptions() { AllowTrailingCommas = true });
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
