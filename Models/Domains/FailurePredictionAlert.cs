namespace IMS_API_.Models.Domains;

public class FailurePredictionAlert
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid? VehiclePartId { get; set; }
    public required string PredictionSummary { get; set; }
    public DateTime? PredictedFailureDate { get; set; }
    public decimal ConfidenceScore { get; set; }
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
    public VehiclePart? VehiclePart { get; set; }
}
