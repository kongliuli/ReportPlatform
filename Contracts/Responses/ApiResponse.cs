namespace Xinglin.WebReportEditor.Contracts.Responses;

public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "操作成功")
    {
        return new ApiResponse<T> { Code = 200, Message = message, Data = data };
    }

    public static ApiResponse<T> Fail(string message, int code = 500)
    {
        return new ApiResponse<T> { Code = code, Message = message, Data = default };
    }
}
