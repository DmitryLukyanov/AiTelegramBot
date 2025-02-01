namespace AiConnector
{
    public interface IAiApiClient
    {
        Task<string> GetResponse(string prompt);
        Task<string> GetChatCompletion(string prompt);
    }
}
