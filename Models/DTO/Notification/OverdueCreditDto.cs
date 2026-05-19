namespace IMS_API_.Models.DTO.Notification;

public class OverdueCreditDto
{
    public Guid CustomerId { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public Guid InvoiceId { get; set; }
    public required string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal DueAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? CreditDueDate { get; set; }
    public int DaysOverdue { get; set; }
}
