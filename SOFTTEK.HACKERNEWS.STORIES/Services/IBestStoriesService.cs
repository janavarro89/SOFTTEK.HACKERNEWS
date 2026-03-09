using SOFTTEK.HACKERNEWS.STORIES.Models;

namespace SOFTTEK.HACKERNEWS.STORIES.Services
{
    public interface IBestStoriesService
    {
        Task<IReadOnlyList<StoryResponse>> GetBestStoriesAsync(int count, CancellationToken cancellationToken);
    }
}
