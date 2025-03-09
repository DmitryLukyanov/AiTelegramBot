namespace AiConnector.SemanticKernel.MongoDb
{
    public class MemoryCollection(string databaseName, string collectionName, string? index = null)
    {
        public string CollectionName => collectionName;
        public string DatabaseName => databaseName;
        public string? Index => index;

        // TODO: move logic?
    }
}
