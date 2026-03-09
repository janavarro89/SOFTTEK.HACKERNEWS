using Microsoft.Extensions.Logging;
using SOFTTEK.HACKERNEWS.APPLICATION.Contracts;
using SOFTTEK.HACKERNEWS.DOMAIN.Entities;
using SOFTTEK.HACKERNEWS.INFRASTRUCTURE.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOFTTEK.HACKERNEWS.INFRASTRUCTURE.Gateways
{
    internal sealed class RemoteHackerNewsGateway : IHackerNewsGateway
    {
        private readonly IHackerNewsApiClient _apiClient;
        private readonly ILogger<RemoteHackerNewsGateway> _logger;

        public RemoteHackerNewsGateway(IHackerNewsApiClient apiClient, ILogger<RemoteHackerNewsGateway> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken cancellationToken)
            => _apiClient.GetBestStoryIdsAsync(cancellationToken);

        public async Task<Story?> GetStoryAsync(long id, CancellationToken cancellationToken)
        {
            var item = await _apiClient.GetItemAsync(id, cancellationToken);

            if (item is null)
            {
                _logger.LogDebug("Hacker News item {StoryId} returned null.", id);
                return null;
            }

            if (!string.Equals(item.Type, "story", StringComparison.Ordinal))
            {
                _logger.LogDebug("Hacker News item {StoryId} was ignored because type was {Type}.", id, item.Type);
                return null;
            }

            if (string.IsNullOrWhiteSpace(item.Title))
            {
                _logger.LogDebug("Hacker News item {StoryId} was ignored because title is missing.", id);
                return null;
            }

            return new Story
            {
                Title = item.Title,
                Uri = item.Url ?? string.Empty,
                PostedBy = item.By ?? string.Empty,
                Time = DateTimeOffset.FromUnixTimeSeconds(item.Time),
                Score = item.Score,
                CommentCount = item.Descendants
            };
        }
    }
}
