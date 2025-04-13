using Microsoft.SemanticKernel;
using System.ComponentModel;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Plugins
{
    public class CreatePollPlugin(TelegramBotClient botClient, ILogger<CreatePollPlugin> logger)
    {
        private const string FunctionName = "CreatePoll";
        private readonly Guid _pluginId = Guid.NewGuid();

        [KernelFunction(FunctionName), Description("Create a poll with provided configuration for the specified chatId.")]
        public async Task PostAsync(
            [Description("The id of the chat that should create a poll")]string chatId,
            [Description("The pool question")] string question,
            [Description("The user created request")] string userName,
            [Description("The list of pool's options")] string[] options)
        {
            LogInformation($"{FunctionName} is being called");

            try
            {
                _ = await botClient.SendPoll(chatId: new ChatId(chatId), question, options.Select(i => new InputPollOption(i)), isAnonymous: false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                throw;
            }

            LogInformation($"{FunctionName} has been successfully called");
        }

        private void LogInformation(string message) => logger.LogInformation("Plugin Id: {0}. {1}", _pluginId, message);
        private void LogError(string message, Exception? ex = null) => logger.LogError(ex, "Plugin Id: {0}. {1}", _pluginId, message);
    }
}
