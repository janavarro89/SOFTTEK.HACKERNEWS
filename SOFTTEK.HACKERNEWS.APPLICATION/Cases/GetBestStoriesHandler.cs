using Microsoft.Extensions.Logging;
using SOFTTEK.HACKERNEWS.APPLICATION.Contracts;
using SOFTTEK.HACKERNEWS.DOMAIN.Entities;

namespace SOFTTEK.HACKERNEWS.APPLICATION.Cases
{
    public sealed class GetBestStoriesHandler : IGetBestStoriesHandler
    {
        private readonly IHackerNewsGateway _gateway;
        private readonly ILogger<GetBestStoriesHandler> _logger;

        public GetBestStoriesHandler(IHackerNewsGateway gateway, ILogger<GetBestStoriesHandler> logger)
        {
            _gateway = gateway;
            _logger = logger;
        }

        public async Task<IReadOnlyList<Story>> HandleAsync(GetBestStoriesRequest request, CancellationToken cancellationToken)
        {
            if (request.Count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(request.Count), "Count must be greater than zero.");
            }

            var ids = await _gateway.GetBestStoryIdsAsync(cancellationToken);

            if (ids.Count == 0)
            {
                _logger.LogWarning("Hacker News returned no best story ids.");
                return Array.Empty<Story>();
            }

            // Fetch more than n to protect against non-story items, deleted items, or missing URLs.
            var candidateIds = ids.Take(Math.Min(ids.Count, Math.Max(request.Count * 3, request.Count))).ToArray();

            _logger.LogInformation("Fetching {CandidateCount} candidate stories to return the best {RequestedCount}.", candidateIds.Length, request.Count);

            var storyTasks = candidateIds.Select(id => _gateway.GetStoryAsync(id, cancellationToken));
            var stories = await Task.WhenAll(storyTasks);

            return stories
                .Where(static story => story is not null)
                .Select(static story => story!)
                .OrderByDescending(static story => story.Score)
                .ThenByDescending(static story => story.Time)
                .Take(request.Count)
                .ToArray();
        }
    }

}
