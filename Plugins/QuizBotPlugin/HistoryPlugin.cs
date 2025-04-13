using AiConnector.SemanticKernel.Mongodb.History;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using static AiConnector.SemanticKernel.Mongodb.History.HistoryHelper;

namespace QuizBotPlugin
{
    public sealed class HistoryPlugin(
        Kernel kernel,
        HistoryHelper historyHelper,
        ILogger<QuizBotPlugin> logger)
    {
        private const string GetHistoryFunctionName = "GetHistoryPlugin";
        private readonly IPromptTemplateFactory _promptTemplateFactory = new KernelPromptTemplateFactory();
        private readonly Guid _pluginId = Guid.NewGuid();

        [KernelFunction(GetHistoryFunctionName), Description("Search messages based on user's query.")]
        [return: Description(@"The messages from history")]
        public async Task<IEnumerable<HistoryRecord>> GetMessagesHistory(
            [Description(@"The search request to query chat history records. 
To get this information, look at the last message from the chat history")] string searchRequest
            )
        {
            LogInformation($"{GetHistoryFunctionName} is being called");

            IEnumerable<HistoryRecord> results;
            try
            {
                searchRequest = "get dm_lk messages?";
                var renderedPrompt = await _promptTemplateFactory.Create(new PromptTemplateConfig(template: searchRequest)).RenderAsync(kernel);
                results = await historyHelper.GetHistory(renderedPrompt, CancellationToken.None);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                throw;
            }

            LogInformation($"{GetHistoryFunctionName} has been successfully called");

            return results;
        }

        private void LogInformation(string message) => logger.LogInformation("Plugin Id: {0}. {1}", _pluginId, message);
        private void LogError(string message, Exception? ex = null) => logger.LogError(ex, "Plugin Id: {0}. {1}", _pluginId, message);
    }
}
