# SOFTTEK.HACKERNEWS

Using ASP.NET Core, implement a RESTful API to retrieve the details of the best n stories from the Hacker News API, as determined by their score, where n is
specified by the caller to the API.

## Requirements

- .NET 8 SDK

## Run locally

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/SOFTTEK.HACKERNEWS.API
```

## API

### Request

```http
GET /api/stories/best?n=10
```

### Success response

```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  }
]
```

### Example validation error

```json
{
  "code": "invalid_request",
  "message": "The query parameter 'n' must be greater than 0.",
  "details": "Try a value such as /api/stories/best?n=10",
  "traceId": "00-..."
}
```

## Configuration

`src/SOFTTEK.HACKERNEWS.API/appsettings.json`

```json
"HackerNews": {
  "BaseUrl": "https://hacker-news.firebaseio.com/v0/",
  "BestStoriesTtlSeconds": 60,
  "StoryDetailsTtlMinutes": 10,
  "MaxConcurrentRequests": 16,
  "CandidateMultiplier": 3,
  "MaximumCandidates": 200
}
```

## Design notes

- `Application` owns the use case.
- `Infrastructure` owns Hacker News integration, caching, resilience, and configuration.
- `Api` owns HTTP transport, response models, and middleware.
- Best story IDs and individual stories are cached independently.
- Story detail fetches are throttled with `SemaphoreSlim` to reduce upstream pressure.
- A resilience handler is attached to the `HttpClient`.
- Invalid input and unexpected failures are mapped to friendly JSON responses.

## Assumptions

- Only items with `type = "story"` are returned.
- If a story has no URL, the API returns an empty string for `uri`.
- If `n <= 0`, the API returns `400 Bad Request`.
- To avoid unnecessary upstream load, the handler fetches a bounded candidate set rather than always requesting every best story in detail.

## Future improvements

- Use a distributed cache such as Redis for multi-instance deployments.
- Add integration tests with `WebApplicationFactory`.
- Add metrics and tracing with OpenTelemetry.
- Return `ProblemDetails` consistently if the target platform standardizes on that response contract.
- Consider stale-while-revalidate behavior to serve warm data during upstream incidents.
