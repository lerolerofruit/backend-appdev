namespace IMS_API_.Models.Domains;

public class PurchaseInvoiceItem
{
    public Guid Id { get; set; }
    public Guid PurchaseInvoiceId { get; set; }
    public Guid VehiclePartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }

    public PurchaseInvoice PurchaseInvoice { get; set; } = null!;
    public VehiclePart VehiclePart { get; set; } = null!;
}
