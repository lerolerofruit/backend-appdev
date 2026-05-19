namespace IMS_API_.Models.DTO.Staff;

public class HighSpenderReportDto
{
    public Guid CustomerId { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public decimal TotalSpend { get; set; }
    public int InvoiceCount { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
}
