namespace IMS_API_.Models.DTO.Auth;

public class StaffSummaryDto
{
    public Guid Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
}
