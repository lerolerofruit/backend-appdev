namespace IMS_API_.Models.DTO.Customer;

public class CustomerProfileDto
{
    public Guid CustomerId { get; set; }
    public Guid UserId { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public decimal OutstandingCredit { get; set; }
    public decimal TotalSpend { get; set; }
}
