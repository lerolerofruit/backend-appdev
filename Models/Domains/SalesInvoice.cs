using System.ComponentModel.DataAnnotations.Schema;

namespace IMS_API_.Models.Domains;

public class SalesInvoice
{
    public Guid Id { get; set; }
    public required string InvoiceNumber { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProcessedByStaffId { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    // Subtotal before any invoice-level loyalty discount

    [NotMapped]
    public decimal SubtotalAmount { get; set; }
    // Loyalty discount applied to the invoice (e.g. 10% when eligible)
    [NotMapped]
    public decimal LoyaltyDiscountAmount { get; set; }
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
