namespace AiConnector.SemanticKernel.ChromaDb
{
    public class MemorySettings
    {
        public const string ConfigurationKey = "Chroma";

        public required string Endpoint { get; set; }
        public required string EmbeddingModel { get; set; }
    }
}
