namespace IMS_API_.Models.DTO.Staff;

public class PendingCreditReportDto
{
    public Guid CustomerId { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal DueAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal OutstandingCredit { get; set; }
    public DateTime? CreditDueDate { get; set; }
}
