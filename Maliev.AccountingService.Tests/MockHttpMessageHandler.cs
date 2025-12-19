using System.Net;

namespace Maliev.AccountingService.Tests;

/// <summary>
/// Mock HTTP message handler for testing HTTP client interactions.
/// Replaces WireMock with a simpler, in-process HTTP mocking solution.
/// Allows configuring responses for specific request patterns.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, (HttpStatusCode StatusCode, string Content)> _responses = new();
    private readonly List<HttpRequestMessage> _requests = new();

    /// <summary>
    /// Configures a mock response for a specific URL pattern.
    /// </summary>
    public void AddResponse(string urlPattern, HttpStatusCode statusCode, string content)
    {
        _responses[urlPattern] = (statusCode, content);
    }

    /// <summary>
    /// Gets all HTTP requests that were sent through this handler.
    /// </summary>
    public IReadOnlyList<HttpRequestMessage> Requests => _requests.AsReadOnly();

    /// <summary>
    /// Clears all configured responses and recorded requests.
    /// </summary>
    public void Reset()
    {
        _responses.Clear();
        _requests.Clear();
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Record the request
        _requests.Add(request);

        // Find matching response
        var url = request.RequestUri?.ToString() ?? string.Empty;
        var matchingResponse = _responses.FirstOrDefault(r => url.Contains(r.Key));

        if (matchingResponse.Key != null)
        {
            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = matchingResponse.Value.StatusCode,
                Content = new StringContent(matchingResponse.Value.Content)
            });
        }

        // Default 404 response if no match found
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound,
            Content = new StringContent("Mock response not configured")
        });
    }

    /// <summary>
    /// Verifies that a request matching the URL pattern was sent.
    /// </summary>
    public bool VerifyRequestSent(string urlPattern)
    {
        return _requests.Any(r => r.RequestUri?.ToString().Contains(urlPattern) == true);
    }

    /// <summary>
    /// Gets the number of requests matching the URL pattern.
    /// </summary>
    public int GetRequestCount(string urlPattern)
    {
        return _requests.Count(r => r.RequestUri?.ToString().Contains(urlPattern) == true);
    }
}
