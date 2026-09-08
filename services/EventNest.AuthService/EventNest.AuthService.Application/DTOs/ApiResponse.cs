namespace EventNest.AuthService.Application.DTOs;

public class ApiResponse<T>
{
    public int Code { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Result { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }

    public static ApiResponse<T> Ok(T result, string? message = null)
    {
        return new ApiResponse<T>
        {
            Code = 200,
            Success = true,
            Message = message,
            Result = result
        };
    }

    public static ApiResponse<T> Fail(int code, string message, Dictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>
        {
            Code = code,
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
