using IMS_API_.Models.DTO.Appointment;
using IMS_API_.Models.DTO.Auth;

namespace IMS_API_.Repositories.Interface;

public interface IAppointmentRepository
{
    // Customer methods
    Task<AuthOperationResult<AppointmentDto>> CreateAppointmentAsync(Guid customerUserId, CreateAppointmentDto request);
    Task<IReadOnlyCollection<AppointmentDto>> GetMyAppointmentsAsync(Guid customerUserId);
    Task<AuthOperationResult<AppointmentDto>> UpdateAppointmentAsync(Guid customerUserId, Guid appointmentId, UpdateAppointmentDto request);
    Task<AuthOperationResult<string>> DeleteAppointmentAsync(Guid customerUserId, Guid appointmentId);

    // Staff methods
    Task<IReadOnlyCollection<StaffAppointmentDto>> GetAllAppointmentsAsync();
    Task<StaffAppointmentDto?> GetAppointmentByIdAsync(Guid appointmentId);
    Task<AuthOperationResult<StaffAppointmentDto>> UpdateAppointmentStatusAsync(Guid appointmentId, UpdateAppointmentStatusDto request);
    Task<AuthOperationResult<string>> DeleteAppointmentAsync(Guid appointmentId);
}
