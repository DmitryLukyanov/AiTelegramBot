using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace AiConnector.OpenAi.SemanticKernel
{
    public static class OpenAiBootstrapper
    {
        public static void ConfigureModel(HostApplicationBuilder builder)
        {
            builder.Services.Configure<OpenAiModelSettings>(builder.Configuration.GetSection(OpenAiModelSettings.ConfigurationKey));

            builder.Services.AddTransient<IAiApiClient, OpenAiClient>();

            builder.Services.AddKeyedSingleton<Kernel>(OpenAiModelSettings.ConfigurationKey, (sr, s) =>
                {
                    var settings = sr.GetRequiredService<IOptions<OpenAiModelSettings>>().Value;
                    var apiKey = settings.ApiKey ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new InvalidOperationException("OPENAI_API_KEY must be configured");
                    var kernelBuilder = Kernel.CreateBuilder()
                        .AddOpenAIChatCompletion(settings.ModelName, apiKey);
                    kernelBuilder.AddOpenAITextEmbeddingGeneration(settings.EmbeddingModel, apiKey);
                    return kernelBuilder.Build();
                })
                .ActivateKeyedSingleton<Kernel>(OpenAiModelSettings.ConfigurationKey);

            builder.Services.AddSingleton((sr) =>
            {
                var kernel = sr.GetRequiredKeyedService<Kernel>(OpenAiModelSettings.ConfigurationKey);
                return kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            });

            builder.Services.AddSingleton((sr) =>
            {
                var kernel = sr.GetRequiredKeyedService<Kernel>(OpenAiModelSettings.ConfigurationKey);
                return kernel.GetRequiredService<IChatCompletionService>();
            });
        }
    }
}
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.