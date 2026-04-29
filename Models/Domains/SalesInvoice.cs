namespace IMS_API_.Models.Domains;

public class SalesInvoice
{
    public Guid Id { get; set; }
    public required string InvoiceNumber { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProcessedByStaffId { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public bool IsCreditSale { get; set; }
    public InvoicePaymentStatus PaymentStatus { get; set; } = InvoicePaymentStatus.Unpaid;
    public DateTime? CreditDueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? EmailSentTo { get; set; }

    public Customer Customer { get; set; } = null!;
    public AppUser ProcessedByStaff { get; set; } = null!;
    public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
    public ICollection<CreditPayment> CreditPayments { get; set; } = new List<CreditPayment>();
}
