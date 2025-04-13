using AiConnector.SemanticKernel.MongoDb;

namespace AiConnector.SemanticKernel.Mongodb.History
{
    public class HistoryHelper(IMemoryClient memoryClient)
    {
        private static readonly string _database = "embedding"; // TODO: replace on metadata once the index is created
        private static readonly string _history = "myCvMemory"; // TODO: replace on history once the index is created
        private readonly MemoryCollection _memoryCollection = new MemoryCollection(_database, _history);

        public async Task SaveHistory(
            long messageId,
            long chatId,
            string user,
            string text,
            DateTime dateTime,
            bool botInvolved,
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, object> metadata = new()
            {
                { "type", "text" },
                { "tags", new List<string>() { "telegram_bot" } },
                { "chatId", chatId.ToString() },
                { "author", user }
            };
            var additionalMetadata = System.Text.Json.JsonSerializer.Serialize(metadata);

            await memoryClient.Save(
                _memoryCollection,
                text: text,
                id: messageId.ToString(),
                description: $"{text}. Context: {additionalMetadata}",
                metadata: additionalMetadata,
                cancellationToken);
        }

        public async Task<IEnumerable<HistoryRecord>> GetHistory(
            string query,
            CancellationToken cancellationToken = default)
        {
            var result = await memoryClient.Search(
                _memoryCollection,
                query: query,
                limit: 30,
                cancellationToken: cancellationToken);
            return result
                .Select(i => 
                {
                    var metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(i.AdditionalMetadata);
                    return new HistoryRecord(i.Text, i.Description, metadata!["author"]!.ToString()!, metadata!["chatId"]!.ToString()!);
                })
                .ToList();
        }

        public record HistoryRecord(string Text, string Description, string UserName, string ChatId);
    }
}
