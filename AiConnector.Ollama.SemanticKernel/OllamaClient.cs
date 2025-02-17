using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace AiConnector.Ollama.SemanticKernel
{
    public class OllamaClient([FromKeyedServices(OllamaModelSettings.ConfigurationKey)] Kernel kernel) : IAiApiClient<string[]>
    {
        public Task<string> GetChatCompletion(string[] chatConversation)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetResponse(string prompt)
        {
            var result = await kernel.InvokePromptAsync(prompt);
            return result.GetValue<string>() ?? string.Empty;
        }
    }
}
