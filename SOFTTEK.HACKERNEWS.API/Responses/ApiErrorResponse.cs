namespace SOFTTEK.HACKERNEWS.API.Responses
{
    public sealed record ApiErrorResponse(
    string Code,
    string Message,
    string? Details = null,
    string? TraceId = null);

}
