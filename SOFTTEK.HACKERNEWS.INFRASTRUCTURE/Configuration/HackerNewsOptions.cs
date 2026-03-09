
using System.ComponentModel.DataAnnotations;

namespace SOFTTEK.HACKERNEWS.INFRASTRUCTURE.Configuration
{
    public sealed class HackerNewsOptions
    {
        public const string SectionName = "HackerNews";

        [Required]
        [Url]
        public string BaseUrl { get; init; } = "https://hacker-news.firebaseio.com/v0/";

        [Range(1, 500)]
        public int BestStoriesTtlSeconds { get; init; } = 60;

        [Range(1, 1440)]
        public int StoryDetailsTtlMinutes { get; init; } = 10;

        [Range(1, 128)]
        public int MaxConcurrentRequests { get; init; } = 16;

        [Range(1, 500)]
        public int CandidateMultiplier { get; init; } = 3;

        [Range(1, 2000)]
        public int MaximumCandidates { get; init; } = 200;

    }

}
