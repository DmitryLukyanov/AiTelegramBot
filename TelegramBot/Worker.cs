namespace TelegramBot;

using AiConnector;
using AiConnector.SemanticKernel.Mongodb.History;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Extensions;

public class Worker(
    ILogger<Worker> logger,
    IAiApiClient<ChatHistory> aiApiClient,
    TelegramBotClient botClient,
    IPluginInitialize initializer,
    HistoryHelper historyHelper) : BackgroundService
{
    private readonly ChatHistory _conversation = [];
    private readonly Guid _workerId = Guid.NewGuid();
    private readonly string[] _botNames = ["@letsthinkaboutbotnameagain_bot", "@ai_bot"];

    public override void Dispose()
    {
        LogInformation("Dispose the bot");

        botClient.OnMessage -= BotOnMessage;
        botClient.OnError -= BotOnError;
        botClient.Close().GetAwaiter().GetResult();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogInformation("Starting the bot..");

        if (!string.IsNullOrWhiteSpace(initializer.PromptsPath))
        {
            _conversation.AddSystemMessagesFromDirectory(initializer.PromptsPath);
        }

        var me = await botClient.GetMe(stoppingToken);

        LogInformation($"Bot Id: {me.Id}, Bot Name: {me.FirstName}");
        botClient.OnMessage += BotOnMessage;
        botClient.OnError += BotOnError;
        LogInformation("Telegram Bot started.");

        await initializer.Initialize();

        while (!stoppingToken.IsCancellationRequested)
        {
            LogInformation($"Worker running at: {DateTimeOffset.UtcNow}");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }

        LogInformation("Telegram Bot stopped.");
    }

    private Task BotOnError(Exception exception, HandleErrorSource source)
    {
        return Task.CompletedTask;
    }

    private async Task BotOnMessage(Message message, UpdateType type)
    {
        LogInformation($"Handling message: {message.Text}. Message Id: {message.Id}. Message type: {type}");
        bool saveOutputToConversation = true;
        var inputTextMessage = message.Text;

        if (message.Voice != null)
        {
            await botClient.SendChatAction(message.Chat.Id, ChatAction.UploadVoice);
            var file = await botClient.GetFile(message.Voice.FileId!);
            using var destination = new MemoryStream();
            await botClient.DownloadFile(file.FilePath!, destination: destination);
            inputTextMessage = await aiApiClient.GetTextFromAudio(destination!, "en", prompt: "The text has been told by captain america");
        }
        var botInvolved = !string.IsNullOrEmpty(message.Text) && _botNames.Any(i => inputTextMessage!.StartsWith(i));
        await SaveHistory(message.Chat.Username!, inputTextMessage!, message.Date, botInvolved);

        _conversation.AddSystemMessage(@$"The current telegram ""chat id"" (also can be called as ""chat id"" is {message.Chat.Id})");

        if (botInvolved || message.Voice != null)
        {
            var enrichedMessage = @$"{inputTextMessage}.
The user name: {message.Chat.Username}
The chat Id: {message.Chat.Id}";
            _conversation.AddUserMessage(enrichedMessage!);
            string response;
            try
            {
                TimeSpan llmTimeout = TimeSpan.FromSeconds(30);
                Task<string> actionTask;
                using CancellationTokenSource cts = new CancellationTokenSource();
                var resultTask = Task.WhenAny(
                    (actionTask = aiApiClient.GetChatCompletion(_conversation, cts.Token)),
                    (Task.Delay(llmTimeout)));
                while (!resultTask.IsCompleted)
                {
                    await botClient.SendChatAction(message.Chat.Id, ChatAction.Typing);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                if (resultTask.Result == actionTask && !resultTask.IsFaulted)
                {
                    response = actionTask.Result;
                }
                else
                {
                    cts.Cancel();
                    saveOutputToConversation = false;
                    response = resultTask.Exception != null ? $"LLM request failed: {resultTask.Exception}" : $"LLM response exceed timeout {llmTimeout}";
                }
            }
            catch (Exception ex)
            {
                LogError(ex, ex.Message);
                throw;
            }

            if (!string.IsNullOrEmpty(response))
            {
                LogInformation($"Sending message to client: {message.Text}. Message Id: {message.Id}. Message type: {type}. SaveMessageToConversation: {saveOutputToConversation}");
                if (saveOutputToConversation)
                {
                    _conversation.AddAssistantMessage(response);
                }
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: response);
                LogInformation($"Sent message to client: {message.Text}. Message Id: {message.Id}. Message type: {type}");
                await SaveHistory("bot", response, DateTime.UtcNow, botInvolved: true);
            }
        }

        LogInformation($"Handled message: {message.Text}. Message Id: {message.Id}. Message type: {type}");

        async Task SaveHistory(string userName, string text, DateTime date, bool botInvolved)
        {
            try
            {
                await historyHelper.SaveHistory(userName, text, date, botInvolved);
            }
            catch (Exception ex)
            {
                // TODO: move this out of here
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"The error processing: {ex.Message}");
                LogError(ex, ex.Message);
                throw;
            }
        }
    }

    private void LogInformation(string message) => logger.LogInformation("Worker Id: {0}. {1}", _workerId, message);
    private void LogError(Exception ex, string message) => logger.LogError(ex, "Worker Id: {0}. {1}", _workerId, message);
}