using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SOFTTEK.HACKERNEWS.STORIES.Models;

namespace SOFTTEK.HACKERNEWS.STORIES.Services
{
    public class BestStoriesService
    {
        private const string BestStoriesCacheKey = "best-stories-snapshot";

        private readonly IHackerNewsClient _client;
        private readonly IMemoryCache _cache;
        private readonly HackerNewsOptions _options;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private readonly SemaphoreSlim _requestThrottle;

        public BestStoriesService(IHackerNewsClient client, IMemoryCache cache, IOptions<HackerNewsOptions> options)
        {
            _client = client;
            _cache = cache;
            _options = options.Value;
            _requestThrottle = new SemaphoreSlim(_options.MaxConcurrentRequests, _options.MaxConcurrentRequests);
        }

        public async Task<IReadOnlyList<StoryResponse>> GetBestStoriesAsync(int count, CancellationToken cancellationToken)
        {
            var snapshot = await GetOrRefreshSnapshotAsync(cancellationToken);
            return snapshot.Take(count).ToArray();
        }

        private async Task<IReadOnlyList<StoryResponse>> GetOrRefreshSnapshotAsync(CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue<IReadOnlyList<StoryResponse>>(BestStoriesCacheKey, out var cachedSnapshot) && cachedSnapshot is not null)
            {
                return cachedSnapshot;
            }

            await _refreshLock.WaitAsync(cancellationToken);
            try
            {
                if (_cache.TryGetValue<IReadOnlyList<StoryResponse>>(BestStoriesCacheKey, out cachedSnapshot) && cachedSnapshot is not null)
                {
                    return cachedSnapshot;
                }

                var storyIds = await _client.GetBestStoryIdsAsync(cancellationToken);

                var tasks = storyIds.Select(id => GetStoryAsync(id, cancellationToken));
                var stories = await Task.WhenAll(tasks);

                var orderedStories = stories
                    .Where(story => story is not null)
                    .Cast<StoryResponse>()
                    .OrderByDescending(story => story.Score)
                    .ThenByDescending(story => story.Time)
                    .ToArray();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.BestStoriesCacheMinutes)
                };

                _cache.Set(BestStoriesCacheKey, orderedStories, cacheOptions);
                return orderedStories;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private async Task<StoryResponse?> GetStoryAsync(int id, CancellationToken cancellationToken)
        {
            var storyCacheKey = $"story:{id}";
            if (_cache.TryGetValue<StoryResponse>(storyCacheKey, out var cachedStory) && cachedStory is not null)
            {
                return cachedStory;
            }

            await _requestThrottle.WaitAsync(cancellationToken);
            try
            {
                if (_cache.TryGetValue<StoryResponse>(storyCacheKey, out cachedStory) && cachedStory is not null)
                {
                    return cachedStory;
                }

                var item = await _client.GetItemAsync(id, cancellationToken);
                if (item is null || !string.Equals(item.Type, "story", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(item.Title) || string.IsNullOrWhiteSpace(item.By) || string.IsNullOrWhiteSpace(item.Url))
                {
                    return null;
                }

                var story = new StoryResponse
                {
                    Title = item.Title,
                    Uri = item.Url,
                    PostedBy = item.By,
                    Time = DateTimeOffset.FromUnixTimeSeconds(item.Time),
                    Score = item.Score ?? 0,
                    CommentCount = item.Descendants ?? 0
                };

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.StoryCacheMinutes)
                };

                _cache.Set(storyCacheKey, story, cacheOptions);
                return story;
            }
            finally
            {
                _requestThrottle.Release();
            }
        }
    }
}
