using Microsoft.SemanticKernel.ChatCompletion;

namespace TelegramBot.Extensions
{
    internal static class MessagesExtensions
    {
        // TODO: find a built-in approach
        public static void AddSystemMessageFromFile(this ChatHistory chatHistory, string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }
            var message = File.ReadAllText(filePath);
            chatHistory.AddSystemMessage(message);
        }
    }
}
