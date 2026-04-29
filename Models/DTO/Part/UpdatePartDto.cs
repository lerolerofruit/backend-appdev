namespace IMS_API_.Models.DTO.Part;

public class UpdatePartDto
{
    public required string PartNumber { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid? PrimaryVendorId { get; set; }
    public decimal UnitPrice { get; set; }
    public int ReorderLevel { get; set; } = 10;
}
