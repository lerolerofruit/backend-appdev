using IMS_API_.Models.Domains;

namespace IMS_API_.Models.DTO.Appointment;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public required string VehicleNumber { get; set; }
    public DateTime AppointmentDate { get; set; }
    public required string ServiceType { get; set; }
    public string? Notes { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
