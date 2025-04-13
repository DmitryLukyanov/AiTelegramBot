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

public class Worker : BackgroundService
{
    private const int MinChatHistorySize = 50;
    private const int MaxLettersCount = 1000;
    private readonly IChatHistoryReducer _reducer = new ChatHistoryTruncationReducer(targetCount: MinChatHistorySize, thresholdCount: 50);
    private readonly WeeklyTimer _weeklyTimer;

    private ChatHistory _conversation = [];
    private readonly Guid _workerId = Guid.NewGuid();
    private readonly string[] _botNames = ["@letsthinkaboutbotnameagain_bot", "@ai_bot", "aibot"];
    private readonly ILogger<Worker> logger;
    private readonly IAiApiClient<ChatHistory> aiApiClient;
    private readonly TelegramBotClient botClient;
    private readonly IPluginInitialize initializer;
    private readonly HistoryHelper historyHelper;

    // TODO: find a better approach
    private long? _chatId;

    public Worker(
        ILogger<Worker> logger,
        IAiApiClient<ChatHistory> aiApiClient,
        TelegramBotClient botClient,
        IPluginInitialize initializer,
        HistoryHelper historyHelper)
    {
        this.logger = logger;
        this.aiApiClient = aiApiClient;
        this.botClient = botClient;
        this.initializer = initializer;
        this.historyHelper = historyHelper;
        this._weeklyTimer = new WeeklyTimer(
            async () => 
            {
                if (_chatId.HasValue)
                {
                    await this.botClient.SendMessage(
                        _chatId.Value,
                        "https://media2.giphy.com/media/v1.Y2lkPTc5MGI3NjExODM4YzR5ZGhtaXRxZ2VlNnd6a3p0b2pucHRmaXNsaGsxeHJzc3FhbiZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/MbnRuf09tjxvk0iIZo/giphy.gif");

                    var upcomingGame = (await QuizBotPlugin.QuizBotPlugin.GetQuizGamesRelatedDetails()).OrderByDescending(i => i.Date).FirstOrDefault();
                    if (upcomingGame != null)
                    {
                        await this.botClient.SendPoll(_chatId.Value!, upcomingGame.Title!, ["Yes !", "Not Yes"]);
                    }
                }
            },
            timeOfDay: TimeSpan.FromHours(2),
            DayOfWeek.Monday);

    }

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

        _chatId = message.Chat.Id; // supposed that the chat is a single, TODO: find a better approach

        if (message.Voice != null)
        {
            if (message.Voice.FileSize > 1048576 /* 1 mb*/)
            {
                await botClient.SendMessage(message.Chat.Id, "https://vgif.ru/gifs/133/vgif-ru-13169.gif");
                return;
            }

            await botClient.SendChatAction(message.Chat.Id, ChatAction.UploadVoice);
            var file = await botClient.GetFile(message.Voice.FileId!);
            using var destination = new MemoryStream();
            await botClient.DownloadFile(file.FilePath!, destination: destination);
            inputTextMessage = await aiApiClient.GetTextFromAudio(destination!, "en", prompt: "The text has been told by captain america");
        }
        var botInvolved = !string.IsNullOrEmpty(message.Text) && _botNames.Any(i => inputTextMessage!.StartsWith(i));
        await SaveHistory(message.Chat.Username!, inputTextMessage!, message.Date, botInvolved);

        _conversation.AddSystemMessage(@$"The current telegram ""chat id"" (also can be called as ""chat id"" is {message.Chat.Id})");

        if (botInvolved)
        {
            if (inputTextMessage!.Length > MaxLettersCount)
            {
                await botClient.SendMessage(message.Chat.Id, "https://media.tenor.com/9vFIpZJPVPwAAAAM/she-talk-too-much-mo%27nique.gif");
                return;
            }

            var enrichedMessage = @$"{inputTextMessage}.
The user name: {message.Chat.Username}
The chat Id: {message.Chat.Id}";
            _conversation.AddUserMessage(enrichedMessage!);
            string response;
            try
            {
                TimeSpan llmTimeout = TimeSpan.FromSeconds(60);
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

        if (_conversation.Count >= MinChatHistorySize)
        {
            _conversation = new ChatHistory(messages: (await _reducer!.ReduceAsync(_conversation))!);
        }

        LogInformation($"Handled message: {message.Text}. Message Id: {message.Id}. Message type: {type}");

        async Task SaveHistory(string userName, string text, DateTime date, bool botInvolved)
        {
            try
            {
                await historyHelper.SaveHistory(message.Id, message.Chat.Id, userName, text, date, botInvolved);
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