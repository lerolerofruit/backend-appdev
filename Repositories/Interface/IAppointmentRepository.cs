using IMS_API_.Models.DTO.Appointment;
using IMS_API_.Models.DTO.Auth;

namespace IMS_API_.Repositories.Interface;

public interface IAppointmentRepository
{
    Task<AuthOperationResult<AppointmentDto>> CreateAppointmentAsync(Guid customerUserId, CreateAppointmentDto request);
    Task<IReadOnlyCollection<AppointmentDto>> GetMyAppointmentsAsync(Guid customerUserId);
}
