using Microsoft.AspNetCore.Mvc;
using SOFTTEK.HACKERNEWS.API.Responses;
using SOFTTEK.HACKERNEWS.APPLICATION.Cases;
using SOFTTEK.HACKERNEWS.APPLICATION.Contracts;

namespace SOFTTEK.HACKERNEWS.API.Controllers
{
    [ApiController]
    [Route("api/stories")]
    public class StoriesController : ControllerBase
    {
        private readonly IGetBestStoriesHandler _handler;
        private readonly ILogger<StoriesController> _logger;

        public StoriesController(IGetBestStoriesHandler handler, ILogger<StoriesController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet("best")]
        [ProducesResponseType(typeof(IReadOnlyList<BestStoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<BestStoryResponse>>> GetBestStories(
            [FromQuery] int n = 10,
            CancellationToken cancellationToken = default)
        {
            if (n <= 0)
            {
                return BadRequest(new ApiErrorResponse(
                    Code: "invalid_request",
                    Message: "The query parameter 'n' must be greater than 0.",
                    Details: "Try a value such as /api/stories/best?n=10",
                    TraceId: HttpContext.TraceIdentifier));
            }

            _logger.LogInformation("Received request for the best {StoryCount} stories.", n);

            var stories = await _handler.HandleAsync(new GetBestStoriesRequest(n), cancellationToken);

            var response = stories
                .Select(story => new BestStoryResponse(
                    story.Title,
                    story.Uri,
                    story.PostedBy,
                    story.Time,
                    story.Score,
                    story.CommentCount))
                .ToArray();

            return Ok(response);
        }

    }
}
