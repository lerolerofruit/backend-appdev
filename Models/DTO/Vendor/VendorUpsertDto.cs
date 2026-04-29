namespace IMS_API_.Models.DTO.Vendor;

public class VendorUpsertDto
{
    public required string Name { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}
