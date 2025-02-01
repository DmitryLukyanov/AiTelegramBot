namespace AiConnector.Ollama.SemanticKernel
{
    public class OllamaModelSettings
    {
        public const string ConfigurationKey = "Ollama";

        public required string ModelName { get; set; }
        public required string Url { get; set; }
    }
}
