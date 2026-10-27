namespace ITManagement.API.Responses;

/// <summary>
/// Response chuẩn cho mọi API endpoint.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }

    public static ApiResponse<T> Ok(T data, string message = "Thành công.") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, IReadOnlyDictionary<string, string[]>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message = "Thành công.") =>
        new() { Success = true, Message = message };

    public static new ApiResponse Fail(string message, IReadOnlyDictionary<string, string[]>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}
