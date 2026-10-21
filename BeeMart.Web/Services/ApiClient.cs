using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace BeeMart.Web.Services;

/// <summary>
/// Typed HTTP client implementation for consuming BeeMart.Api REST endpoints.
/// Automatically forwards the user's JWT Bearer token stored in the authentication cookie.
/// </summary>
public class ApiClient : IApiClient
{
    public const string JwtClaimType = "jwt_token";
    public const string TokenClaimType = "Token";
    public const string AccessTokenName = "access_token";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        var relativePath = NormalizeEndpoint(endpoint);
        using var request = new HttpRequestMessage(HttpMethod.Get, relativePath);
        await AttachBearerTokenAsync(request);

        return await SendAndUnwrapAsync<T>(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        var relativePath = NormalizeEndpoint(endpoint);
        using var request = new HttpRequestMessage(HttpMethod.Post, relativePath)
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };
        await AttachBearerTokenAsync(request);

        return await SendAndUnwrapAsync<TResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApiResponse<TResponse>> PostAsync<TResponse>(
        string endpoint,
        object? payload,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<object?, TResponse>(endpoint, payload, cancellationToken);
    }

    private async Task AttachBearerTokenAsync(HttpRequestMessage request)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var token = httpContext.User.FindFirst(JwtClaimType)?.Value
                    ?? httpContext.User.FindFirst(TokenClaimType)?.Value
                    ?? httpContext.User.FindFirst(AccessTokenName)?.Value;

        if (string.IsNullOrWhiteSpace(token))
        {
            token = await httpContext.GetTokenAsync(AccessTokenName);
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<ApiResponse<T>> SendAndUnwrapAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var statusCode = (int)response.StatusCode;
            var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(rawJson))
            {
                try
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(rawJson, JsonOptions);
                    if (apiResponse is not null)
                    {
                        apiResponse.StatusCode = statusCode;
                        if (!response.IsSuccessStatusCode && string.IsNullOrWhiteSpace(apiResponse.Status))
                        {
                            apiResponse.Status = "error";
                        }

                        return apiResponse;
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogWarning(
                        jsonEx,
                        "Failed to deserialize ApiResponse<{TypeName}> from {RequestUri} (HTTP {StatusCode}).",
                        typeof(T).Name,
                        request.RequestUri,
                        statusCode);
                }
            }

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<T>
                {
                    Status = "success",
                    StatusCode = statusCode
                };
            }

            return ApiResponse<T>.Error(
                statusCode == 401
                    ? "Unauthorized request. Please log in again."
                    : $"Backend API returned HTTP {statusCode}.",
                statusCode);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP communication error calling {RequestUri}.", request.RequestUri);
            return ApiResponse<T>.Error(
                "Unable to connect to BeeMart Backend API. Please ensure BeeMart.Api is running.",
                503);
        }
        catch (TaskCanceledException cancelEx) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(cancelEx, "HTTP request timed out calling {RequestUri}.", request.RequestUri);
            return ApiResponse<T>.Error("Request to BeeMart Backend API timed out.", 504);
        }
    }

    private static string NormalizeEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return string.Empty;
        }

        return endpoint.TrimStart('/');
    }
}
