using AiConnector.SemanticKernel.ChromaDb;
using AiConnector.SemanticKernel.OpenAi;
using Telegram.Bot;
using TelegramBot;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
