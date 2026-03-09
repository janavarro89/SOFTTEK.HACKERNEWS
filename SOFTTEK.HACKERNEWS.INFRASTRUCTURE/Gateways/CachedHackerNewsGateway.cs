using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SOFTTEK.HACKERNEWS.APPLICATION.Contracts;
using SOFTTEK.HACKERNEWS.DOMAIN.Entities;
using SOFTTEK.HACKERNEWS.INFRASTRUCTURE.Configuration;

namespace SOFTTEK.HACKERNEWS.INFRASTRUCTURE.Gateways
{
    public sealed class CachedHackerNewsGateway : IHackerNewsGateway, IDisposable
    {
        private const string BestStoriesCacheKey = "hn:beststoryids";
        private const string StoryCacheKeyPrefix = "hn:story:";
        private readonly IMemoryCache _cache;
        private readonly IHackerNewsGateway _inner;
        private readonly HackerNewsOptions _options;
        private readonly ILogger<CachedHackerNewsGateway> _logger;
        private readonly SemaphoreSlim _bestStoriesRefreshLock = new(1, 1);
        private readonly SemaphoreSlim _storyThrottle;

        public CachedHackerNewsGateway(
            IMemoryCache cache,
            IHackerNewsGateway inner,
            IOptions<HackerNewsOptions> options,
            ILogger<CachedHackerNewsGateway> logger)
        {
            _cache = cache;
            _inner = inner;
            _options = options.Value;
            _logger = logger;
            _storyThrottle = new SemaphoreSlim(_options.MaxConcurrentRequests, _options.MaxConcurrentRequests);
        }

        public async Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue<IReadOnlyList<long>>(BestStoriesCacheKey, out var cachedIds) && cachedIds is not null)
            {
                _logger.LogDebug("Best story ids served from cache.");
                return cachedIds;
            }

            await _bestStoriesRefreshLock.WaitAsync(cancellationToken);
            try
            {
                if (_cache.TryGetValue<IReadOnlyList<long>>(BestStoriesCacheKey, out cachedIds) && cachedIds is not null)
                {
                    return cachedIds;
                }

                var ids = await _inner.GetBestStoryIdsAsync(cancellationToken);

                _cache.Set(BestStoriesCacheKey, ids, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.BestStoriesTtlSeconds),
                    Priority = CacheItemPriority.High,
                    Size = 1
                });

                _logger.LogInformation("Cached {Count} best story ids for {TtlSeconds} seconds.", ids.Count, _options.BestStoriesTtlSeconds);

                return ids;
            }
            finally
            {
                _bestStoriesRefreshLock.Release();
            }
        }

        public async Task<Story?> GetStoryAsync(long id, CancellationToken cancellationToken)
        {
            var cacheKey = $"{StoryCacheKeyPrefix}{id}";
            if (_cache.TryGetValue<Story>(cacheKey, out var cachedStory) && cachedStory is not null)
            {
                _logger.LogDebug("Story {StoryId} served from cache.", id);
                return cachedStory;
            }

            await _storyThrottle.WaitAsync(cancellationToken);
            try
            {
                if (_cache.TryGetValue<Story>(cacheKey, out cachedStory) && cachedStory is not null)
                {
                    return cachedStory;
                }

                var story = await _inner.GetStoryAsync(id, cancellationToken);
                if (story is null)
                {
                    return null;
                }

                _cache.Set(cacheKey, story, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.StoryDetailsTtlMinutes),
                    Priority = CacheItemPriority.Normal,
                    Size = 1
                });

                return story;
            }
            finally
            {
                _storyThrottle.Release();
            }
        }

        public void Dispose()
        {
            _bestStoriesRefreshLock.Dispose();
            _storyThrottle.Dispose();
        }
    }
}
