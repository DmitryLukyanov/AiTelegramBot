using AiConnector.SemanticKernel.Mongodb.History;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Embeddings;
using MongoDB.Driver;
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace AiConnector.SemanticKernel.MongoDb
{
    public static class MemoryBootstrapper
    {
        public static void Configure(IHostApplicationBuilder builder)
        {
            builder.Services.Configure<MemorySettings>(builder.Configuration.GetSection(MemorySettings.ConfigurationKey));
            builder.Services.AddSingleton<IMemoryClient, MemoryClient>(sr => 
            {
                var mongoClient = sr.GetRequiredService<IMongoClient>();
                var embedding = sr.GetRequiredService<ITextEmbeddingGenerationService>();
                return new MemoryClient(mongoClient, embedding);
            });
            builder.Services.AddSingleton<HistoryHelper>();

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            });
            builder.Services.AddSingleton<IMongoClient>(sr => 
            {
                var settings = sr.GetRequiredService<IOptions<MemorySettings>>().Value;
                var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? settings.Endpoint;
                var mongoClientSettings = MongoClientSettings.FromConnectionString(connectionString);
                mongoClientSettings.LoggingSettings = new MongoDB.Driver.Core.Configuration.LoggingSettings(loggerFactory);
                return new MongoClient(mongoClientSettings);
            });
        }
    }
}
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
