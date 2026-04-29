namespace IMS_API_.Models.Domains;

public class Customer
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? RegisteredByStaffId { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public decimal TotalSpend { get; set; }
    public decimal OutstandingCredit { get; set; }
    public DateTime? CreditDueDate { get; set; }

    public AppUser User { get; set; } = null!;
    public AppUser? RegisteredByStaff { get; set; }
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
    public ICollection<ServiceAppointment> ServiceAppointments { get; set; } = new List<ServiceAppointment>();
    public ICollection<CustomerReview> Reviews { get; set; } = new List<CustomerReview>();
    public ICollection<PartRequest> PartRequests { get; set; } = new List<PartRequest>();
    public ICollection<FailurePredictionAlert> PredictionAlerts { get; set; } = new List<FailurePredictionAlert>();
}
