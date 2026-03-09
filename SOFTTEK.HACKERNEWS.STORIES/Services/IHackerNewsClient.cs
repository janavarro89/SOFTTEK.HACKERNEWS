using SOFTTEK.HACKERNEWS.STORIES.Models;

namespace SOFTTEK.HACKERNEWS.STORIES.Services
{
    public interface IHackerNewsClient
    {
        Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken);
        Task<HackerNewsItemResponse?> GetItemAsync(int id, CancellationToken cancellationToken);
    }
}
