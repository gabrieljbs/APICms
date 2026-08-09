namespace Shared.DTOs.Common;

public class ApiResponseDto<T>
{
    public string Status { get; set; } = "success";
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ApiResponseDto<T> Success(T data, string? message = null) => new()
    {
        Status = "success",
        Message = message,
        Data = data
    };

    public static ApiResponseDto<T> Error(string message) => new()
    {
        Status = "error",
        Message = message,
        Data = default
    };
}
