using MongoDB.Bson;
using MongoDB.Driver;

namespace AiConnector.SemanticKernel.Mongodb.History
{
    public class HistoryHelper(IMongoClient mongoClient)
    {
        private readonly string _database = "metadata";
        private readonly string _history = "history";

        public async Task SaveHistory(string user, string text, DateTime dateTime, bool botInvolved)
        {
            var database = mongoClient.GetDatabase(_database);
            await database.CreateCollectionAsync(_history, new CreateCollectionOptions { Capped = true, MaxDocuments = 5000, MaxSize = 10000 });
            var collection = database.GetCollection<BsonDocument>(_history);
            await collection.InsertOneAsync(BsonDocument.Parse(@$"{{ text : '{
                text
                    .Replace(":", @"""")
                    .Replace("'", @"""")}', user : '{user}', created : '{dateTime}', bot_involved : '{botInvolved}' }}"));
        }
    }
}
