using System.Security.Claims;
using IMS_API_.Models.DTO.Appointment;
using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/customer/appointments")]
[Authorize(Roles = "Customer")]
public class CustomerAppointmentsController(IAppointmentRepository appointmentRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyAppointments()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized("Invalid token.");
        }

        var appointments = await appointmentRepository.GetMyAppointmentsAsync(userId.Value);
        return Ok(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> BookAppointment(CreateAppointmentDto request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized("Invalid token.");
        }

        var result = await appointmentRepository.CreateAppointmentAsync(userId.Value, request);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Message);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
