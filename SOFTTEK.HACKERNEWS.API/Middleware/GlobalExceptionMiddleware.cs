using SOFTTEK.HACKERNEWS.API.Responses;
using System.Net;
using System.Text.Json;

namespace SOFTTEK.HACKERNEWS.API.Middleware
{
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                _logger.LogWarning(exception, "Validation error for request {TraceId}.", context.TraceIdentifier);
                await WriteErrorAsync(context, HttpStatusCode.BadRequest, new ApiErrorResponse(
                    Code: "invalid_request",
                    Message: exception.Message,
                    Details: "Review the query parameters and try again.",
                    TraceId: context.TraceIdentifier));
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "Upstream HTTP error for request {TraceId}.", context.TraceIdentifier);
                await WriteErrorAsync(context, HttpStatusCode.BadGateway, new ApiErrorResponse(
                    Code: "upstream_error",
                    Message: "We could not retrieve data from Hacker News right now.",
                    Details: "Please try again in a moment.",
                    TraceId: context.TraceIdentifier));
            }
            catch (TaskCanceledException exception) when (!context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogError(exception, "Upstream timeout for request {TraceId}.", context.TraceIdentifier);
                await WriteErrorAsync(context, HttpStatusCode.GatewayTimeout, new ApiErrorResponse(
                    Code: "upstream_timeout",
                    Message: "The request to Hacker News took too long.",
                    Details: "Please try again shortly.",
                    TraceId: context.TraceIdentifier));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled error for request {TraceId}.", context.TraceIdentifier);
                await WriteErrorAsync(context, HttpStatusCode.InternalServerError, new ApiErrorResponse(
                    Code: "internal_error",
                    Message: "Something went wrong while processing the request.",
                    Details: "If the problem persists, contact support and include the trace id.",
                    TraceId: context.TraceIdentifier));
            }
        }

        private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, ApiErrorResponse response)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
