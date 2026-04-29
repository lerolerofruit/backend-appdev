namespace IMS_API_.Models.Domains;

public class VehiclePart
{
    public Guid Id { get; set; }
    public required string PartNumber { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid? PrimaryVendorId { get; set; }
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Vendor? PrimaryVendor { get; set; }
    public ICollection<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; } = new List<PurchaseInvoiceItem>();
    public ICollection<SalesInvoiceItem> SalesInvoiceItems { get; set; } = new List<SalesInvoiceItem>();
    public ICollection<FailurePredictionAlert> PredictionAlerts { get; set; } = new List<FailurePredictionAlert>();
}
