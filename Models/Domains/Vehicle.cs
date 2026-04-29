namespace IMS_API_.Models.Domains;

public class Vehicle
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public required string VehicleNumber { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public string? Vin { get; set; }
    public int Mileage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;
    public ICollection<ServiceAppointment> ServiceAppointments { get; set; } = new List<ServiceAppointment>();
    public ICollection<FailurePredictionAlert> PredictionAlerts { get; set; } = new List<FailurePredictionAlert>();
}
