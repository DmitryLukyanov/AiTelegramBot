namespace TelegramBot;

using AiConnector;
using Microsoft.SemanticKernel.ChatCompletion;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class Worker(
    // TODO: use scopes?
    ILogger<Worker> logger,
    IAiApiClient<ChatHistory> aiApiClient,
    TelegramBotClient botClient,
    AiBotInitializer initializer) : BackgroundService
{
    private readonly ChatHistory _conversation = [];
    private readonly Guid _workerId = Guid.NewGuid();

    public override void Dispose()
    {
        LogInformation("Dispose the bot");

        botClient.OnMessage -= BotOnMessage;
        botClient.Close().GetAwaiter().GetResult();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogInformation("Starting the bot..");

        _conversation.AddSystemMessage(@"You are a knowledgeable and resourceful assistant whose primary task 
is to offer comprehensive, factual, and well-structured information about Dmitry Lukyanov’s career. 
Provide details about his professional background, roles, achievements, and relevant qualifications in a clear, 
concise manner. Focus on verifiable information and reliable sources while avoiding speculation or personal opinions. Please do not add anything other than mentioned in the CV.

If the question asks about any details that are not mentioned in his CV, please response with this link https://i.gifer.com/EgFH.mp4 without any other text");

        var me = await botClient.GetMe(stoppingToken);

        LogInformation($"Bot Id: {me.Id}, Bot Name: {me.FirstName}");
        botClient.OnMessage += BotOnMessage;
        LogInformation("Telegram Bot started.");

        await initializer.Initialize();

        while (!stoppingToken.IsCancellationRequested)
        {
            LogInformation($"Worker running at: {DateTimeOffset.UtcNow}");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }

        LogInformation("Telegram Bot stopped.");
    }

    private async Task BotOnMessage(Message message, UpdateType type)
    {
        LogInformation($"Handling message: {message.Text}. Message Id: {message.Id}. Message type: {type}");

        // TODO: refactoring
        var inputTextMessage = message.Text;
        if (message.Voice != null)
        {
            await botClient.SendChatAction(message.Chat.Id, ChatAction.UploadVoice);
            var file = await botClient.GetFile(message.Voice.FileId!);
            using var destination = new MemoryStream();
            await botClient.DownloadFile(file.FilePath!, destination: destination);
            inputTextMessage = await aiApiClient.GetTextFromAudio(destination!, "en", "The text has been told by captain america");
        }

        if (!string.IsNullOrEmpty(message.Text) && (inputTextMessage!.StartsWith("@ai_bot") || (inputTextMessage!.Contains("letsthinkaboutbotnameagain_bot"))) || message.Voice != null)
        {
            _conversation.AddUserMessage(inputTextMessage! /*, message.Id.ToString()*/);
            string response;
            try
            {
                TimeSpan llmTimeout = TimeSpan.FromSeconds(30);
                Task<string> actionTask;
                Task delayTask;
                var resultTask = Task.WhenAny(
                    (actionTask = aiApiClient.GetChatCompletion(_conversation)),
                    (delayTask = Task.Delay(llmTimeout)));
                while (!resultTask.IsCompleted)
                {
                    await botClient.SendChatAction(message.Chat.Id, ChatAction.Typing);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                if (resultTask.Result == actionTask)
                {
                    response = actionTask.Result;
                }
                else
                {
                    response = $"LLM response exceed timeout {llmTimeout}";
                }
            }
            catch (Exception ex)
            {
                LogError(ex, ex.Message);
                throw;
            }

            if (!string.IsNullOrEmpty(response))
            {
                LogInformation($"Sending message to client: {message.Text}. Message Id: {message.Id}. Message type: {type}");
                _conversation.AddAssistantMessage(response);
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: response);
                LogInformation($"Sent message to client: {message.Text}. Message Id: {message.Id}. Message type: {type}");
            }
        }

        LogInformation($"Handled message: {message.Text}. Message Id: {message.Id}. Message type: {type}");
    }

    private void LogInformation(string message) => logger.LogInformation("Worker Id: {0}. {1}", _workerId, message);
    private void LogError(Exception ex, string message) => logger.LogError(ex, "Worker Id: {0}. {1}", _workerId, message);
}