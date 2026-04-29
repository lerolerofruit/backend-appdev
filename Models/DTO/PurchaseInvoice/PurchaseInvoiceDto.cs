namespace IMS_API_.Models.DTO.PurchaseInvoice;

public class PurchaseInvoiceDto
{
    public Guid Id { get; set; }
    public required string InvoiceNumber { get; set; }
    public Guid VendorId { get; set; }
    public required string VendorName { get; set; }
    public Guid CreatedByAdminId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchaseInvoiceItemDto> Items { get; set; } = [];
}
