using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace AiConnector.SemanticKernel.OpenAi
{
    public static class OpenAiBootstrapper
    {
        public static void ConfigureModel(HostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IAiApiClient<ChatHistory>, OpenAiClient>();
            builder.Services.Configure<OpenAiModelSettings>(builder.Configuration.GetSection(OpenAiModelSettings.ConfigurationKey));

            var openAiSection = builder.Configuration.GetSection(OpenAiModelSettings.ConfigurationKey);
            var settings = openAiSection.Get<OpenAiModelSettings>()!;

            var apiKey = settings.ApiKey ?? 
                Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new InvalidOperationException("OPENAI_API_KEY must be configured");

            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder
                .AddOpenAIChatCompletion(settings.ModelName, apiKey)
                .AddOpenAITextEmbeddingGeneration(settings.EmbeddingModel, apiKey)
                .AddOpenAIAudioToText(modelId: settings.AudioModel, apiKey: apiKey);

            kernelBuilder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            builder.Services.Add(kernelBuilder.Services);

            builder.Services.AddSingleton<Kernel>((sr) => kernelBuilder.Build());
        }
    }
}
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.