namespace IMS_API_.Models.Domains;

public class CreditPayment
{
    public Guid Id { get; set; }
    public Guid SalesInvoiceId { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime PaidOn { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public SalesInvoice SalesInvoice { get; set; } = null!;
}
