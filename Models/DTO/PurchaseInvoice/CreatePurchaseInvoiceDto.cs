namespace IMS_API_.Models.DTO.PurchaseInvoice;

public class CreatePurchaseInvoiceDto
{
    public required string InvoiceNumber { get; set; }
    public Guid VendorId { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public List<CreatePurchaseInvoiceItemDto> Items { get; set; } = [];
}
