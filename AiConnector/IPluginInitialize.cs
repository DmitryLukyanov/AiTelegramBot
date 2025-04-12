namespace AiConnector
{
    public interface IPluginInitialize
    {
        Task Initialize();

        string? PromptsPath { get; } 
    }
}
