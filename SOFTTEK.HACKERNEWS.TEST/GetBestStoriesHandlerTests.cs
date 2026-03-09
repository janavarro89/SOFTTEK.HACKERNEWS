using Microsoft.Extensions.Logging.Abstractions;
using SOFTTEK.HACKERNEWS.APPLICATION.Cases;
using SOFTTEK.HACKERNEWS.APPLICATION.Contracts;
using SOFTTEK.HACKERNEWS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace SOFTTEK.HACKERNEWS.UTEST
{
    public sealed class GetBestStoriesHandlerTests
    {
        [Fact]
        public async Task HandleAsync_Returns_TopStoriesOrderedByScoreThenTime()
        {
            var gateway = new Mock<IHackerNewsGateway>();
            gateway.Setup(x => x.GetBestStoryIdsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new long[] { 1, 2, 3 });

            gateway.Setup(x => x.GetStoryAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Story
                {
                    Title = "One",
                    Uri = "https://example.com/1",
                    PostedBy = "jorge",
                    Time = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    Score = 100,
                    CommentCount = 10
                });

            gateway.Setup(x => x.GetStoryAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Story
                {
                    Title = "Two",
                    Uri = "https://example.com/2",
                    PostedBy = "adrian",
                    Time = new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero),
                    Score = 200,
                    CommentCount = 20
                });

            gateway.Setup(x => x.GetStoryAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Story
                {
                    Title = "Three",
                    Uri = "https://example.com/3",
                    PostedBy = "george",
                    Time = new DateTimeOffset(2025, 1, 3, 0, 0, 0, TimeSpan.Zero),
                    Score = 200,
                    CommentCount = 30
                });

            var sut = new GetBestStoriesHandler(gateway.Object, NullLogger<GetBestStoriesHandler>.Instance);

            var result = await sut.HandleAsync(new GetBestStoriesRequest(2), CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.Equal("Three", result[0].Title);
            Assert.Equal("Two", result[1].Title);
        }

        [Fact]
        public async Task HandleAsync_Throws_WhenCountIsInvalid()
        {
            var gateway = new Mock<IHackerNewsGateway>();
            var sut = new GetBestStoriesHandler(gateway.Object, NullLogger<GetBestStoriesHandler>.Instance);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                sut.HandleAsync(new GetBestStoriesRequest(0), CancellationToken.None));
        }
    }
}
