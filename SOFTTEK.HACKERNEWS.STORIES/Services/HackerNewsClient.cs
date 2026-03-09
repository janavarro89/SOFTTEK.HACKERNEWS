using Microsoft.Extensions.Options;
using SOFTTEK.HACKERNEWS.STORIES.Models;
using System.Text.Json;

namespace SOFTTEK.HACKERNEWS.STORIES.Services
{
    public class HackerNewsClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient _httpClient;
        private readonly HackerNewsOptions _options;

        public HackerNewsClient(HttpClient httpClient, IOptions<HackerNewsOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync(_options.BestStoriesEndpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var ids = await JsonSerializer.DeserializeAsync<List<int>>(stream, JsonOptions, cancellationToken);
            return ids ?? [];
        }

        public async Task<HackerNewsItemResponse?> GetItemAsync(int id, CancellationToken cancellationToken)
        {
            var endpoint = string.Format(_options.ItemEndpointTemplate, id);
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<HackerNewsItemResponse>(stream, JsonOptions, cancellationToken);
        }
    }
}
