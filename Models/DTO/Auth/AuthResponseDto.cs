namespace IMS_API_.Models.DTO.Auth;

public class AuthResponseDto
{
    public required string Token { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
}
