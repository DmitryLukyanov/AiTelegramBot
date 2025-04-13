using AiConnector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using System.Reflection;

namespace QuizBotPlugin
{
    public sealed class QuizBotBootstrapper
    {
        public static void ConfigureServices(IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<QuizBotPlugin>();
            builder.Services.AddSingleton<IPluginInitialize, QuizBotInitializer>();
        }

        public static void InitializeKernel(Kernel kernel, IServiceProvider services)
        {
            kernel.Plugins.AddFromType<QuizBotPlugin>("QuizBot", services);
        }
    }

    public class QuizBotInitializer() : IPluginInitialize
    {
        public string? PromptsPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location)!, "Prompts/System");

        public Task Initialize() => Task.CompletedTask; // no actions yet
    }
}
