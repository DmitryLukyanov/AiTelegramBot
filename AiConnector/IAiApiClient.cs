namespace AiConnector
{
    public interface IAiApiClient<TConversation>
    {
        Task<string> GetChatCompletion(TConversation chatConversation, CancellationToken cancellationToken);
        Task<string> GetTextFromAudio(MemoryStream voice, string language, string prompt);
    }
}
