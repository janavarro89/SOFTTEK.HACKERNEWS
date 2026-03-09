using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SOFTTEK.HACKERNEWS.INFRASTRUCTURE.Clients
{
    internal sealed class HackerNewsApiClient: IHackerNewsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HackerNewsApiClient> _logger;

        public HackerNewsApiClient(HttpClient httpClient, ILogger<HackerNewsApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Fetching best story ids from Hacker News.");

            var ids = await _httpClient.GetFromJsonAsync<List<long>>("beststories.json", cancellationToken);

            return ids is { Count: > 0 } ? ids : Array.Empty<long>();
        }

        public async Task<HackerNewsItemDto?> GetItemAsync(long id, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Fetching Hacker News item {StoryId}.", id);
            return await _httpClient.GetFromJsonAsync<HackerNewsItemDto>($"item/{id}.json", cancellationToken);
        }

    }
}
