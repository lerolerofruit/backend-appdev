namespace IMS_API_.Models.DTO.SalesInvoice;

public class CreateSalesInvoiceItemDto
{
    public Guid VehiclePartId { get; set; }
    public int Quantity { get; set; }
    public decimal Discount { get; set; } = 0m;
}
