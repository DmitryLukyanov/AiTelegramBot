using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiConnector.OpenAi.SemanticKernel
{
    public class OpenAiClient(
        [FromKeyedServices(OpenAiModelSettings.ConfigurationKey)] Kernel kernel,
        IChatCompletionService chatCompletionService) : IAiApiClient
    {
        public async Task<string> GetResponse(string prompt)
        {
            var result = await kernel.InvokePromptAsync(prompt);
            return result.GetValue<string>() ?? string.Empty;
        }

        public async Task<string> GetChatCompletion(string prompt)
        {
            var content = await chatCompletionService.GetChatMessageContentAsync(prompt);
            return content.ToString();
        }
    }
}
