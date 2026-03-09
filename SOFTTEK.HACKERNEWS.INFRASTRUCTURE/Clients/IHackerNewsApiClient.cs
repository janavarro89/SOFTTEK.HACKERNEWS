using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOFTTEK.HACKERNEWS.INFRASTRUCTURE.Clients
{
    internal interface IHackerNewsApiClient
    {
        Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken cancellationToken);
        Task<HackerNewsItemDto?> GetItemAsync(long id, CancellationToken cancellationToken);
    }
}
