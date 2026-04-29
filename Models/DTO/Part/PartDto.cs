namespace IMS_API_.Models.DTO.Part;

public class PartDto
{
    public Guid Id { get; set; }
    public required string PartNumber { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid? PrimaryVendorId { get; set; }
    public string? PrimaryVendorName { get; set; }
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
