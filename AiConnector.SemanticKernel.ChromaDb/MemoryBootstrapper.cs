using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Embeddings;
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace AiConnector.SemanticKernel.ChromaDb
{
    public static class MemoryBootstrapper
    {
        public static void Configure(IHostApplicationBuilder builder)
        {
            builder.Services.Configure<MemorySettings>(builder.Configuration.GetSection(MemorySettings.ConfigurationKey));
            builder.Services.AddSingleton<IMemoryClient, MemoryClient>(sr => 
            {
                var chromaClient = sr.GetRequiredService<ChromaClient>();
                var embedding = sr.GetRequiredService<ITextEmbeddingGenerationService>();
                return new MemoryClient(chromaClient, embedding);
            });

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            });
            builder.Services.AddSingleton(sr => 
            {
                var settings = sr.GetRequiredService<IOptions<MemorySettings>>().Value;
                return new ChromaClient(settings.Endpoint, loggerFactory: loggerFactory);
            });
        }
    }
}
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
