namespace IMS_API_.Models.DTO.Auth;

public class LoginRequestDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
