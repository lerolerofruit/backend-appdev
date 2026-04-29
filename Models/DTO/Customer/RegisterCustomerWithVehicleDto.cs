namespace IMS_API_.Models.DTO.Customer;

public class RegisterCustomerWithVehicleDto
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Password { get; set; }
    public required VehicleUpsertDto Vehicle { get; set; }
}
