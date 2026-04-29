namespace IMS_API_.Models.DTO.PurchaseInvoice;

public class PurchaseInvoiceItemDto
{
    public Guid Id { get; set; }
    public Guid VehiclePartId { get; set; }
    public required string PartNumber { get; set; }
    public required string PartName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
}
