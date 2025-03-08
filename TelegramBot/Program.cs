using AiConnector.SemanticKernel.OpenAi;
using AiConnector.SemanticKernel.ChromaDb;
using Telegram.Bot;
using TelegramBot;
Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!! 1");
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton((sp) => 
{
    var telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_API_KEY") ?? throw new InvalidOperationException("TELEGRAM_BOT_API_KEY env variable must be specialized");
    return new TelegramBotClient(telegramBotToken);
});

//OpenAiBootstrapper.ConfigureModel(builder);
//MemoryBootstrapper.Configure(builder);
//AiBotbootStrapper.Configure(builder);

var host = builder.Build();
//AiBotbootStrapper.ConfigureHost(host);
host.Run();