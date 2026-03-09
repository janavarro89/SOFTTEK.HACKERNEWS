using SOFTTEK.HACKERNEWS.DOMAIN.Entities;

namespace SOFTTEK.HACKERNEWS.APPLICATION.Contracts
{
    public interface IHackerNewsGateway
    {
        Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken cancellationToken);
        Task<Story?> GetStoryAsync(long id, CancellationToken cancellationToken);
    }
}
