using IMS_API_.Models.DTO.Appointment;
using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/staff/appointments")]
[Authorize(Roles = "Staff")]
public class StaffAppointmentsController(IAppointmentRepository appointmentRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAppointments()
    {
        var appointments = await appointmentRepository.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointmentById(Guid id)
    {
        var appointment = await appointmentRepository.GetAppointmentByIdAsync(id);
        if (appointment is null)
        {
            return NotFound("Appointment not found.");
        }

        return Ok(appointment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointmentStatus(Guid id, UpdateAppointmentStatusDto request)
    {
        var result = await appointmentRepository.UpdateAppointmentStatusAsync(id, request);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Message);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(Guid id)
    {
        var result = await appointmentRepository.DeleteAppointmentAsync(id);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Message);
    }
}
