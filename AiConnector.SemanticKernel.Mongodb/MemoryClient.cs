using Microsoft.SemanticKernel.Connectors.MongoDB;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using MongoDB.Bson;
using MongoDB.Driver;
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
namespace AiConnector.SemanticKernel.MongoDb
{
    public interface IMemoryClient
    {
        Task<bool> TryInitializeCollection(string collectionName, string path, CancellationToken cancellationToken = default);
        Task Save(MemoryCollection collection, string text, string id, string? description = null, string? metadata = null, CancellationToken cancellationToken = default);
        Task<string[]> Search(MemoryCollection collection, string query, int limit = 1, double minRelevanceScore = 0, CancellationToken cancellationToken = default);
    }

    public class MemoryClient(IMongoClient memoryClient, ITextEmbeddingGenerationService textEmbeddingGenerationService) : IMemoryClient
    {
        private const string EmbeddingDatabase = "embedding";

        /*
 {
    "mappings": {
        "dynamic": true,
        "fields": {
            "embedding": {
                "dimensions": 1536,
                "similarity": "dotProduct",
                "type": "knnVector"
            }
        }
    }
}
         */
        public async Task<bool> TryInitializeCollection(string collectionName, string path, CancellationToken cancellationToken = default)
        {
            var database = memoryClient.GetDatabase(EmbeddingDatabase);
            var collectionsList = await (await database.ListCollectionNamesAsync(cancellationToken: cancellationToken)).ToListAsync(cancellationToken: cancellationToken);
            if (collectionsList.Contains(collectionName, StringComparer.OrdinalIgnoreCase))
            {
                return (await database.GetCollection<BsonDocument>(collectionName).FindAsync($@"{{ _id: {{ $regex: '^{path}' }} }}", cancellationToken: cancellationToken)).FirstOrDefault(cancellationToken: cancellationToken) != null;
            }
            await database.CreateCollectionAsync(collectionName, cancellationToken: cancellationToken);
            return false;
        }

        public async Task Save(MemoryCollection collection, string text, string id, string? description, string? metadata, CancellationToken cancellationToken = default)
        {
            var chromaMemoryStore = new MongoDBMemoryStore(memoryClient, collection.DatabaseName, indexName: collection.Index);
            var memory = new SemanticTextMemory(chromaMemoryStore, textEmbeddingGenerationService);
            await memory.SaveInformationAsync(collection.CollectionName, text, id, description, metadata, cancellationToken: cancellationToken);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<string[]> Search(MemoryCollection collection, string query, int limit = 1, double minRelevanceScore = 0, CancellationToken cancellationToken = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var chromaMemoryStore = new MongoDBMemoryStore(memoryClient, collection.DatabaseName, collection.Index);
            var memory = new SemanticTextMemory(chromaMemoryStore, textEmbeddingGenerationService);
            var enumerable = memory.SearchAsync(collection.CollectionName, query, limit, minRelevanceScore, cancellationToken: cancellationToken);
            return (await enumerable.ToListAsync(cancellationToken: cancellationToken)).Select(r => r.Metadata.Text).ToArray();
        }
    }
}
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
