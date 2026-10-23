using System.Text.Json.Serialization;

namespace BeeMart.Api.Common;

/// <summary>
/// Generic standard JSON API response wrapper.
/// </summary>
/// <typeparam name="T">The payload data type.</typeparam>
public class ApiResponse<T>
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "success";

    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Status = "success",
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Error(string message)
    {
        return new ApiResponse<T>
        {
            Status = "error",
            Message = message,
            Data = default
        };
    }
}

/// <summary>
/// Non-generic standard JSON API response wrapper and factory helper.
/// </summary>
public class ApiResponse : ApiResponse<object?>
{
    public static ApiResponse Success(string? message = null)
    {
        return new ApiResponse
        {
            Status = "success",
            Message = message,
            Data = null
        };
    }

    public static ApiResponse<T> Success<T>(T data, string? message = null)
    {
        return ApiResponse<T>.Success(data, message);
    }

    public new static ApiResponse Error(string message)
    {
        return new ApiResponse
        {
            Status = "error",
            Message = message,
            Data = null
        };
    }
}
