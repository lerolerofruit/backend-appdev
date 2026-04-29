using IMS_API_.Models.Domains;

namespace IMS_API_.Models.DTO.Staff;

public class CustomerInvoiceSummaryDto
{
    public Guid Id { get; set; }
    public required string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsCreditSale { get; set; }
    public InvoicePaymentStatus PaymentStatus { get; set; }
    public DateTime? CreditDueDate { get; set; }
    public DateTime? PaidAt { get; set; }
}
