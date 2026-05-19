namespace IMS_API_.Models.DTO.Notification;

public class LowStockPartDto
{
    public Guid PartId { get; set; }
    public required string PartNumber { get; set; }
    public required string PartName { get; set; }
    public int CurrentStockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public decimal SellingPrice { get; set; }
}
