namespace IMS_API_.Models.DTO.Staff;

public class PendingCreditReportDto
{
    public Guid CustomerId { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public decimal OutstandingCredit { get; set; }
    public DateTime? CreditDueDate { get; set; }
}
