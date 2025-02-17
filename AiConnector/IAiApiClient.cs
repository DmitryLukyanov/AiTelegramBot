namespace AiConnector
{
    public interface IAiApiClient<TConversation>
    {
        Task<string> GetChatCompletion(TConversation chatConversation);
    }
}
