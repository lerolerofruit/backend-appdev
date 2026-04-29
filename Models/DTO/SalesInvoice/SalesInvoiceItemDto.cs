namespace IMS_API_.Models.DTO.SalesInvoice;

public class SalesInvoiceItemDto
{
    public Guid Id { get; set; }
    public Guid VehiclePartId { get; set; }
    public required string PartNumber { get; set; }
    public required string PartName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal LineTotal { get; set; }
}
