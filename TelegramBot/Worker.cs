namespace TelegramBot;

using AiConnector;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class Worker(
    ILogger<Worker> logger, 
    IAiApiClient aiApiClient, 
    TelegramBotClient botClient,
    AiBotInitializer initializer) : BackgroundService
{
    public override void Dispose()
    {
        botClient.OnMessage -= BotOnMessage;
        botClient.Close().GetAwaiter().GetResult();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
            var response = await aiApiClient.GetResponse(message.Text);

            if (!string.IsNullOrEmpty(response))
            {
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: response
                );
            }
        }
    }
}