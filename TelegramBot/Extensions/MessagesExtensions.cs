using Microsoft.SemanticKernel.ChatCompletion;

namespace TelegramBot.Extensions
{
    internal static class MessagesExtensions
    {
        // TODO: find a built-in approach
        public static void AddSystemMessagesFromDirectory(this ChatHistory chatHistory, string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException(directoryPath);
            }
            foreach (var file in Directory.EnumerateFiles(directoryPath))
            {
                var message = File.ReadAllText(file);
                chatHistory.AddSystemMessage(message);
            }
        }
    }
}
