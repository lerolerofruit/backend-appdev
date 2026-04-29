namespace IMS_API_.Models.DTO.Appointment;

public class CreateAppointmentDto
{
    public Guid VehicleId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public required string ServiceType { get; set; }
    public string? Notes { get; set; }
}
