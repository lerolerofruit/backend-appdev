using IMS_API_.Models.DTO.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IMS_API_.Repositories.Interface;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthRepository authRepository) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto request)
    {
        var result = await authRepository.RegisterCustomerAsync(request);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Message);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("register-staff")]
    public async Task<IActionResult> RegisterStaff(RegisterRequestDto request)
    {
        var result = await authRepository.RegisterStaffAsync(request);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Message);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("staff")]
    public async Task<IActionResult> GetStaff()
    {
        var staffUsers = await authRepository.GetStaffAsync();
        return Ok(staffUsers);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("staff/{staffId:guid}/status")]
    public async Task<IActionResult> UpdateStaffStatus(Guid staffId, UpdateStaffStatusDto request)
    {
        var result = await authRepository.UpdateStaffStatusAsync(staffId, request.IsActive);
        if (!result.Succeeded)
        {
            return result.Message == "Staff not found."
                ? NotFound(result.Message)
                : BadRequest(result.Message);
        }

        return Ok(new { Message = result.Message });
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("staff/{staffId:guid}/role")]
    public async Task<IActionResult> UpdateStaffRole(Guid staffId, UpdateStaffRoleDto request)
    {
        var result = await authRepository.UpdateStaffRoleAsync(staffId, request.Role);
        if (!result.Succeeded)
        {
            return result.Message == "Staff not found."
                ? NotFound(result.Message)
                : BadRequest(result.Message);
        }   

        return Ok(new { Message = result.Message });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var result = await authRepository.LoginAsync(request);
        return result.Succeeded ? Ok(result.Data) : Unauthorized(result.Message);
    }
}
