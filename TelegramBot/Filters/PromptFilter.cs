using Microsoft.SemanticKernel;

namespace TelegramBot.Filters
{
    public sealed class PromptFilter(ILogger<PromptFilter> logger) : IPromptRenderFilter
    {
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            logger.LogInformation("FunctionInvoking - {PluginName}.{FunctionName}", context.Function.PluginName, context.Function.Name);

            await next(context);

            if (string.IsNullOrWhiteSpace(context.RenderedPrompt))
            {
                context.RenderedPrompt = "At this point, there is no related information. Please ask again";
            }
        }
    }
}
