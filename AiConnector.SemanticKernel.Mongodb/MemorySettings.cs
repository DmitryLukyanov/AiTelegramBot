namespace AiConnector.SemanticKernel.MongoDb
{
    public class MemorySettings
    {
        public const string ConfigurationKey = "MongoDb";

        public required string Endpoint { get; set; }
        public required string EmbeddingModel { get; set; }
    }
}
