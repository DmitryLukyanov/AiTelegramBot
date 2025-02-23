namespace AiConnector
{
    public interface IAiApiClient<TConversation>
    {
        Task<string> GetChatCompletion(TConversation chatConversation);
        Task<string> GetTextFromAudio(MemoryStream voice, string language, string promt);
    }
}
