using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
namespace AiConnector.SemanticKernel.ChromaDb
{
    public interface IMemoryClient
    {
        Task<MemoryCollection> CreateCollection(string collectionName, CancellationToken cancellationToken = default);
        Task Save(MemoryCollection collection, string text, string id, string? description = null, string? metadata = null, CancellationToken cancellationToken = default);
        Task<string[]> Search(MemoryCollection collection, string query, int limit = 1, double minRelevanceScore = 0, CancellationToken cancellationToken = default);
    }

    public class MemoryClient(ChromaClient memoryClient, ITextEmbeddingGenerationService textEmbeddingGenerationService) : IMemoryClient
    {
        public async Task<MemoryCollection> CreateCollection(string collectionName, CancellationToken cancellationToken = default)
        {
            await memoryClient.CreateCollectionAsync(collectionName, cancellationToken);
            return new MemoryCollection(collectionName);
        }

        public async Task Save(MemoryCollection collection, string text, string id, string? description, string? metadata, CancellationToken cancellationToken = default)
        {
            var chromaMemoryStore = new ChromaMemoryStore(memoryClient);
            var memory = new SemanticTextMemory(chromaMemoryStore, textEmbeddingGenerationService);
            await memory.SaveInformationAsync(collection.CollectionName, text, id, description, metadata, cancellationToken: cancellationToken);
        }

        public async Task<string[]> Search(MemoryCollection collection, string query, int limit = 1, double minRelevanceScore = 0, CancellationToken cancellationToken = default)
        {
            var chromaMemoryStore = new ChromaMemoryStore(memoryClient);
            var memory = new SemanticTextMemory(chromaMemoryStore, textEmbeddingGenerationService);
            var enumerable = memory.SearchAsync(collection.CollectionName, query, limit, minRelevanceScore, cancellationToken: cancellationToken);
            return await enumerable.Select(r => r.Metadata.Text).ToArrayAsync();
        }
    }
}
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
