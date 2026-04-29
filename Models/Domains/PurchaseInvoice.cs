namespace IMS_API_.Models.Domains;

public class PurchaseInvoice
{
    public Guid Id { get; set; }
    public required string InvoiceNumber { get; set; }
    public Guid VendorId { get; set; }
    public Guid CreatedByAdminId { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }

    public Vendor Vendor { get; set; } = null!;
    public AppUser CreatedByAdmin { get; set; } = null!;
    public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
}
