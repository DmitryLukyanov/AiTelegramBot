using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace AiConnector.Ollama.SemanticKernel
{
    public static class OllamaBootstrapper
    {
        public static void ConfigureModel(HostApplicationBuilder builder)
        {
            builder.Services.Configure<OllamaModelSettings>(builder.Configuration.GetSection(OllamaModelSettings.ConfigurationKey));

            builder.Services.AddTransient<IAiApiClient<string[]>, OllamaClient>();
            builder.Services.AddKeyedTransient<Kernel>(OllamaModelSettings.ConfigurationKey, (sp, key) =>
            {
                var settings = sp.GetRequiredService<IOptions<OllamaModelSettings>>().Value;

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                var kernelBuilder = Kernel.CreateBuilder()
                    .AddOllamaTextGeneration(settings.ModelName, new Uri(settings.Url));
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                return kernelBuilder.Build();
            });
        }
    }
}
