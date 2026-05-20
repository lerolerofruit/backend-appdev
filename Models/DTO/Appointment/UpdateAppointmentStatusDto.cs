using IMS_API_.Models.Domains;

namespace IMS_API_.Models.DTO.Appointment;

public class UpdateAppointmentStatusDto
{
    public AppointmentStatus Status { get; set; }
    public string? ServiceNotes { get; set; }
}
