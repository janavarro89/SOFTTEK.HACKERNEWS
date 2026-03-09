using SOFTTEK.HACKERNEWS.APPLICATION.Cases;
using SOFTTEK.HACKERNEWS.DOMAIN.Entities;

namespace SOFTTEK.HACKERNEWS.APPLICATION.Contracts
{
    public interface IGetBestStoriesHandler
    {
        Task<IReadOnlyList<Story>> HandleAsync(GetBestStoriesRequest request, CancellationToken cancellationToken);
    }
}
