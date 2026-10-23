using System.Text.Json.Serialization;

namespace BeeMart.Web.Services;

/// <summary>
/// Standard API response wrapper matching Backend API JSON structure.
/// </summary>
/// <typeparam name="T">The payload data type.</typeparam>
public class ApiResponse<T>
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "error";

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonIgnore]
    public int StatusCode { get; set; }

    [JsonIgnore]
    public bool IsSuccess => string.Equals(Status, "success", StringComparison.OrdinalIgnoreCase);

    public static ApiResponse<T> Success(T data, string? message = null, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Status = "success",
            Message = message,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> Error(string message, int statusCode = 500)
    {
        return new ApiResponse<T>
        {
            Status = "error",
            Message = message,
            Data = default,
            StatusCode = statusCode
        };
    }
}

/// <summary>
/// Typed HTTP API client contract for communicating with BeeMart.Api (/api/v1/...).
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Sends an HTTP GET request to the specified relative API endpoint and unwraps <see cref="ApiResponse{T}"/>.
    /// Automatically attaches the authenticated user's JWT Bearer token when present.
    /// </summary>
    Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP POST request with a strongly-typed JSON payload and unwraps <see cref="ApiResponse{TResponse}"/>.
    /// Automatically attaches the authenticated user's JWT Bearer token when present.
    /// </summary>
    Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest payload,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP POST request with an object payload and unwraps <see cref="ApiResponse{TResponse}"/>.
    /// Automatically attaches the authenticated user's JWT Bearer token when present.
    /// </summary>
    Task<ApiResponse<TResponse>> PostAsync<TResponse>(
        string endpoint,
        object? payload,
        CancellationToken cancellationToken = default);
}
