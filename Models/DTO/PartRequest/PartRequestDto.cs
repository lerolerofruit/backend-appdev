namespace IMS_API_.Models.DTO.PartRequest;

public class PartRequestDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public required string RequestedPartName { get; set; }
    public string? RequestedPartNumber { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedOn { get; set; }
    public DateTime? ResolvedOn { get; set; }
}
