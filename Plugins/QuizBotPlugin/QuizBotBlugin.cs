using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace QuizBotPlugin
{
    public record QuizGameInfo(string Title, string Date, string Time, string Location, string Description);

    public sealed class QuizBotPlugin(
        Kernel kernel,
        ILogger<QuizBotPlugin> logger)
    {
        private const string FunctionName = "QuizBotPlugin";
        private readonly IPromptTemplateFactory _promptTemplateFactory = new KernelPromptTemplateFactory();
        private readonly Guid _pluginId = Guid.NewGuid();

        [KernelFunction(FunctionName), Description("Get information about current and upcomming quiz games.")]
        [return: Description(@"The list of current and upcoming quiz games in a format:
* Title - the quiz game title,
* Date - the date when quiz game is scheduled,
* Time - the time in the day when the game is scheduled
* Location - the location where the game scheduled
* Description - the description of the game
")]
        public async Task<IEnumerable<QuizGameInfo>> GetAsync(KernelArguments arguments)
        {
            LogInformation($"{FunctionName} is being called");

            var prompt = @"Provide all information about current and upcoming quiz games";

            List<QuizGameInfo> results;
            try
            {
                var renderedPrompt = await _promptTemplateFactory.Create(new PromptTemplateConfig(template: prompt)).RenderAsync(kernel, arguments);
                results = await GetQuizGamesRelatedDetails();
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                throw;
            }

            LogInformation($"{FunctionName} has been successfully called");

            return results;
        }

        private void LogInformation(string message) => logger.LogInformation("Plugin Id: {0}. {1}", _pluginId, message);
        private void LogError(string message, Exception? ex = null) => logger.LogError(ex, "Plugin Id: {0}. {1}", _pluginId, message);

        private async Task<List<QuizGameInfo>> GetQuizGamesRelatedDetails()
        {
            var url = "https://quizplease.ru/schedule";
            using var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            // Example: Select all game entries (you'll need to inspect the actual HTML structure)
            var gameNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'schedule-block available')]");
            List<QuizGameInfo> games = new();

            if (gameNodes != null)
            {
                foreach (var gameNode in gameNodes)
                {
                    // Extract relevant information, e.g., date, time, location
                    var title = gameNode.SelectSingleNode(".//div[contains(@class, 'h2-game-card h2-left')]")?.InnerText.Trim();
                    var date = gameNode.SelectSingleNode(".//div[contains(@class, 'h3-mb10')]")?.InnerText.Trim();
                    var time = gameNode.SelectSingleNode(".//div[contains(@class, 'schedule-info')]")?.InnerText.Trim();
                    var location = gameNode.SelectSingleNode(".//div[contains(@class, 'schedule-block-info-bar')]")?.InnerText.Trim();
                    var description = gameNode.SelectSingleNode(".//div[contains(@class, 'schedule-block-top')]")?.InnerText.Trim();
                    games.Add(new (title!, date!, time!, location!, description!));
                }
            }
            return games;
        }
    }
}
