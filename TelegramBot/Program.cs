using AiConnector.SemanticKernel.MongoDb;
using AiConnector.SemanticKernel.OpenAi;
using Microsoft.Extensions.Logging.AzureAppServices;
using Telegram.Bot;
using TelegramBot;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.AddAzureWebAppDiagnostics();
builder.Services.Configure<AzureFileLoggerOptions>(options =>
{
    options.FileName = "azure-diagnostics-";
    options.FileSizeLimit = 50 * 1024;
    options.RetainedFileCountLimit = 5;
});
builder.Services.Configure<AzureBlobLoggerOptions>(options =>
{
    options.BlobName = "log.txt";
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
builder.Services.AddLogging();
builder.Services.AddHealthChecks();
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton((sp) =>
{
    var telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_API_KEY") ?? throw new InvalidOperationException("TELEGRAM_BOT_API_KEY env variable must be specialized");
    return new TelegramBotClient(telegramBotToken);
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
OpenAiBootstrapper.ConfigureModel(builder);
MemoryBootstrapper.Configure(builder);
AiBotbootStrapper.Configure(builder);

var app = builder.Build();
AiBotbootStrapper.ConfigureHost(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health_check");

app.Run();
