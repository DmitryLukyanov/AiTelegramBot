using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiConnector.OpenAi.SemanticKernel
{
    public class OpenAiClient(IChatCompletionService chatCompletionService, Kernel kernel) : IAiApiClient<ChatHistory>
    {
        public async Task<string> GetChatCompletion(ChatHistory chatConversation)
        {
            try
            {
                var content = await chatCompletionService.GetChatMessageContentAsync(
                    chatHistory: chatConversation,
                    new PromptExecutionSettings
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                    }, 
                    kernel);
                return content.ToString();
            }
            catch (Exception ex)
            {
                return $"The processing of the message has been failed with this error: {ex.Message}";
            }
        }
    }
}
