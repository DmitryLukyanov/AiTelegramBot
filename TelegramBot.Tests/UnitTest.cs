namespace TelegramBot.Tests
{
    public class UnitTest
    {
        [Fact]
        public async Task Ensure_chroma_is_Aacessible()
        {
            const string serverUrl = "http://chroma:8000/api/v1/collections";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(serverUrl);

            response.EnsureSuccessStatusCode();
        }
    }
}