namespace IMS_API_.Models.Domains;

public class SalesInvoiceItem
{
    public Guid Id { get; set; }
    public Guid SalesInvoiceId { get; set; }
    public Guid VehiclePartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal LineTotal { get; set; }

    public SalesInvoice SalesInvoice { get; set; } = null!;
    public VehiclePart VehiclePart { get; set; } = null!;
}
