using System.Text.Json.Serialization;

namespace SOFTTEK.HACKERNEWS.INFRASTRUCTURE.Clients
{
    internal sealed class HackerNewsItemDto
    {
        [JsonPropertyName("by")]
        public string? By { get; set; }

        [JsonPropertyName("descendants")]
        public int Descendants { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

}
