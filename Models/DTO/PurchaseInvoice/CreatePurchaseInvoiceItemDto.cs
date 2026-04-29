namespace IMS_API_.Models.DTO.PurchaseInvoice;

public class CreatePurchaseInvoiceItemDto
{
    public Guid VehiclePartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
}
