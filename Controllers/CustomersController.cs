using System.Security.Claims;
using IMS_API_.Models.DTO.Customer;
using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController(ICustomerRepository customerRepository) : ControllerBase
{
    [Authorize(Roles = "Staff")]
    [HttpPost("register-with-vehicle")]
    public async Task<IActionResult> RegisterWithVehicle(RegisterCustomerWithVehicleDto request)
    {
        var staffUserId = GetCurrentUserId();
        if (staffUserId is null)
        {
            return Unauthorized("Invalid token.");
        }

        var result = await customerRepository.RegisterCustomerWithVehicleByStaffAsync(staffUserId.Value, request);
        return result.Succeeded ? Ok(new { Message = result.Message, CustomerId = result.Data }) : BadRequest(result.Message);
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized("Invalid token.");
        }

        var result = await customerRepository.GetCustomerProfileAsync(userId.Value);
        return result.Succeeded ? Ok(result.Data) : NotFound(result.Message);
    }

    [Authorize(Roles = "Customer")]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateCustomerProfileDto request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized("Invalid token.");
        }

        var result = await customerRepository.UpdateCustomerProfileAsync(userId.Value, request);
        return result.Succeeded ? Ok(new { Message = result.Message }) : BadRequest(result.Message);
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("vehicles")]
    public async Task<IActionResult> GetMyVehicles()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized("Invalid token.");
        }

        var vehicles = await customerRepository.GetVehiclesAsync(userId.Value);
        return Ok(vehicles);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("vehicles")]
    public async Task<IActionResult> AddVehicle(VehicleUpsertDto request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized("Invalid token.");
        }

        var result = await customerRepository.AddVehicleAsync(userId.Value, request);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Message);
    }

    [Authorize(Roles = "Customer")]
    [HttpPut("vehicles/{vehicleId:guid}")]
    public async Task<IActionResult> UpdateVehicle(Guid vehicleId, VehicleUpsertDto request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized("Invalid token.");
        }

        var result = await customerRepository.UpdateVehicleAsync(userId.Value, vehicleId, request);
        if (!result.Succeeded)
        {
            return result.Message == "Vehicle not found." ? NotFound(result.Message) : BadRequest(result.Message);
        }

        return Ok(new { Message = result.Message });
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
