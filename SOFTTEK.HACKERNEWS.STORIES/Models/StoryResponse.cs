using System.Text.Json.Serialization;

namespace SOFTTEK.HACKERNEWS.STORIES.Models
{
    public class StoryResponse
    {
        public required string Title { get; init; }
        public required string Uri { get; init; }
        public required string PostedBy { get; init; }
        public required DateTimeOffset Time { get; init; }
        public int Score { get; init; }
        public int CommentCount { get; init; }
        
    }
}
