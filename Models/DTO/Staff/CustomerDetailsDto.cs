using IMS_API_.Models.DTO.Customer;

namespace IMS_API_.Models.DTO.Staff;

public class CustomerDetailsDto
{
    public Guid CustomerId { get; set; }
    public Guid UserId { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public DateTime RegisteredAt { get; set; }
    public decimal TotalSpend { get; set; }
    public decimal OutstandingCredit { get; set; }
    public DateTime? CreditDueDate { get; set; }

    public List<VehicleDto> Vehicles { get; set; } = [];
    public List<CustomerInvoiceSummaryDto> SalesInvoices { get; set; } = [];
}
