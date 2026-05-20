using IMS_API_.Models.Domains;

namespace IMS_API_.Models.DTO.Appointment;

public class StaffAppointmentDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public Guid VehicleId { get; set; }
    public required string VehicleNumber { get; set; }
    public DateTime AppointmentDate { get; set; }
    public required string ServiceType { get; set; }
    public string? Notes { get; set; }
    public string? ServiceNotes { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
