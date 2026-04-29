namespace IMS_API_.Models.DTO.Auth;

public class AuthOperationResult<T>
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
}
