using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiConnector.SemanticKernel.OpenAi
{
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public class OpenAiClient(IChatCompletionService chatCompletionService, IAudioToTextService audioToTextService, Kernel kernel) : IAiApiClient<ChatHistory>
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    {
        public async Task<string> GetChatCompletion(ChatHistory chatConversation, CancellationToken cancellationToken)
        {
            try
            {
                var content = await chatCompletionService.GetChatMessageContentAsync(
                    chatHistory: chatConversation,
                    new PromptExecutionSettings
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                    }, 
                    kernel,
                    cancellationToken);
                return content.ToString();
            }
            catch (Exception ex)
            {
                return $"The processing of the message has been failed with this error: {ex.Message}";
            }
        }

        public async Task<string> GetTextFromAudio(MemoryStream voice, string language, string prompt)
        {
            // Set execution settings (optional)
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            OpenAIAudioToTextExecutionSettings executionSettings = new()
            {
                Language = language, // The language of the audio data as two-letter ISO-639-1 language code (e.g. 'en' or 'es').
                Prompt = prompt, // An optional text to guide the model's style or continue a previous audio segment.
                                          // The prompt should match the audio language.
                ResponseFormat = "json", // The format to return the transcribed text in.
                                         // Supported formats are json, text, srt, verbose_json, or vtt. Default is 'json'.
                Temperature = 0.3f, // The randomness of the generated text.
                                    // Select a value from 0.0 to 1.0. 0 is the default.
            };
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            // Read audio content from a file
            var audioFileBinaryData = new ReadOnlyMemory<byte>(voice.ToArray());
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            AudioContent audioContent = new(audioFileBinaryData, mimeType: null);
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            // Convert audio to text
            try
            {
                var response = await audioToTextService.GetTextContentAsync(audioContent, executionSettings)!;
                return response.Text!;
            }
            catch (Exception ex)
            {
                return $"The processing of the audio request has been failed: {ex.Message}";
            }
        }
    }
}
