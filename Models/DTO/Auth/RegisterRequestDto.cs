namespace IMS_API_.Models.DTO.Auth;

public class RegisterRequestDto
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Password { get; set; }
    public string Role { get; set; } = "Customer";
}
