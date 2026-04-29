namespace IMS_API_.Models.DTO.Customer;

public class VehicleUpsertDto
{
    public required string VehicleNumber { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public string? Vin { get; set; }
    public int Mileage { get; set; }
}
