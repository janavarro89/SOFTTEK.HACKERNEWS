using SOFTTEK.HACKERNEWS.APPLICATION.Contracts;
using SOFTTEK.HACKERNEWS.DOMAIN.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using SOFTTEK.HACKERNEWS.INFRASTRUCTURE.Gateways;
using SOFTTEK.HACKERNEWS.INFRASTRUCTURE.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Microsoft.Extensions.Caching.Memory;

namespace SOFTTEK.HACKERNEWS.UTESTS
{
    public sealed class CachedHackerNewsGatewayTests
    {
        [Fact]
        public async Task GetStoryAsync_Caches_Story()
        {
            var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 100 });
            var inner = new Mock<IHackerNewsGateway>();
            inner.Setup(x => x.GetStoryAsync(42, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Story
                {
                    Title = "Cached story",
                    Uri = "https://example.com",
                    PostedBy = "alice",
                    Time = DateTimeOffset.UtcNow,
                    Score = 123,
                    CommentCount = 4
                });

            var sut = new CachedHackerNewsGateway(
                cache,
                inner.Object,
                Options.Create(new HackerNewsOptions()),
                NullLogger<CachedHackerNewsGateway>.Instance);

            var first = await sut.GetStoryAsync(42, CancellationToken.None);
            var second = await sut.GetStoryAsync(42, CancellationToken.None);

            Assert.NotNull(first);
            Assert.NotNull(second);
            inner.Verify(x => x.GetStoryAsync(42, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
