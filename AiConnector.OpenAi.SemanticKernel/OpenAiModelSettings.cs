namespace AiConnector.OpenAi.SemanticKernel
{
    public class OpenAiModelSettings
    {
        public const string ConfigurationKey = "OpenAi";

        public required string ModelName { get; set; }
        public required string ApiKey { get; set; }
        public required string EmbeddingModel { get; set; }
    }
}
