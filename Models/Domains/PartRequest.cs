namespace IMS_API_.Models.Domains;

public class PartRequest
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public required string RequestedPartName { get; set; }
    public string? RequestedPartNumber { get; set; }
    public int Quantity { get; set; }
    public PartRequestStatus Status { get; set; } = PartRequestStatus.Pending;
    public DateTime RequestedOn { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedOn { get; set; }

    public Customer Customer { get; set; } = null!;
}
