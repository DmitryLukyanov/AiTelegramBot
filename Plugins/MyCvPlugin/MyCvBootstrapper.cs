using AiConnector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using System.Reflection;

namespace MyCvPlugin
{
    public sealed class MyCvBootstrapper
    {
        public static void InitializeKernel(IHost host)
        {
            var kernel = host.Services.GetRequiredService<Kernel>();
            kernel.Plugins.AddFromType<MyCvPlugin>("MyCv", host.Services);
        }

        public static void ConfigureServices(IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<MyCvPlugin>();
            builder.Services.AddSingleton<IPluginInitialize, AiBotInitializer>();
        }
    }

    public class AiBotInitializer(MyCvPlugin myCvPlugin) : IPluginInitialize
    {
        public string? PromptsPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location)!, "Prompts/System");

        public Task Initialize() => myCvPlugin.InitializeMyCv();
    }
}
