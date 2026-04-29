using IMS_API_.Models.Domains;

namespace IMS_API_.Models.DTO.SalesInvoice;

public class SalesInvoiceDto
{
    public Guid Id { get; set; }
    public required string InvoiceNumber { get; set; }
    public Guid CustomerId { get; set; }
    public required string CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public Guid ProcessedByStaffId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsCreditSale { get; set; }
    public InvoicePaymentStatus PaymentStatus { get; set; }
    public DateTime? CreditDueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? EmailSentTo { get; set; }
    public List<SalesInvoiceItemDto> Items { get; set; } = [];
}
