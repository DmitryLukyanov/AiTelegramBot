namespace TelegramBot;

using AiConnector;
using Microsoft.SemanticKernel.ChatCompletion;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class Worker(
    ILogger<Worker> logger, 
    IAiApiClient<ChatHistory> aiApiClient, 
    TelegramBotClient botClient,
    AiBotInitializer initializer) : BackgroundService
{
    private readonly ChatHistory _conversation = [];

    public override void Dispose()
    {
        botClient.OnMessage -= BotOnMessage;
        botClient.Close().GetAwaiter().GetResult();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _conversation.AddSystemMessage(@"You are a knowledgeable and resourceful assistant whose primary task 
is to offer comprehensive, factual, and well-structured information about Dmitry Lukyanov’s career. 
Provide details about his professional background, roles, achievements, and relevant qualifications in a clear, 
concise manner. Focus on verifiable information and reliable sources while avoiding speculation or personal opinions. Please do not add anything other than mentioned in the CV.

If the question asks about any details that are not mentioned in his CV, please response with this link https://i.gifer.com/EgFH.mp4 without any other text");

        var me = await botClient.GetMe(stoppingToken);

        logger.LogInformation($"Bot Id: {me.Id}, Bot Name: {me.FirstName}");
        botClient.OnMessage += BotOnMessage;
        logger.LogInformation("Telegram Bot started.");

        await initializer.Initialize();

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.UtcNow);
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }

        logger.LogInformation("Telegram Bot stopped.");
    }

    private async Task BotOnMessage(Message message, UpdateType type)
    {
        if (!string.IsNullOrEmpty(message.Text) && (message.Text.StartsWith("@ai_bot") || (message.Text.Contains("letsthinkaboutbotnameagain_bot"))))
        {
            _conversation.AddUserMessage(message.Text /*, message.Id.ToString()*/);
            var response = await aiApiClient.GetChatCompletion(_conversation);

            if (!string.IsNullOrEmpty(response))
            {
                _conversation.AddAssistantMessage(response);
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: response
                );
            }
        }
    }
}