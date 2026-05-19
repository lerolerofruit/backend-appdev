using System.Security.Claims;
using IMS_API_.Models.DTO.PartRequest;
using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer")]
public class PartRequestsController(IPartRequestRepository partRequestRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePartRequest(CreatePartRequestDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var customerUserId))
        {
            return Unauthorized("Invalid user context.");
        }

        var result = await partRequestRepository.CreatePartRequestAsync(customerUserId, request);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Message);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyPartRequests()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var customerUserId))
        {
            return Unauthorized("Invalid user context.");
        }

        var requests = await partRequestRepository.GetMyPartRequestsAsync(customerUserId);
        return Ok(requests);
    }

    [Authorize(Roles = "Admin, Staff")]
    [HttpGet]
    public async Task<IActionResult> GetAllPartRequests()
    {
        var requests = await partRequestRepository.GetAllPartRequestsAsync();
        return Ok(requests);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPartRequestById(Guid id)
    {
        var result = await partRequestRepository.GetPartRequestByIdAsync(id);
        return result.Succeeded ? Ok(result.Data) : NotFound(result.Message);
    }

    [Authorize(Roles = "Admin, Staff")]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdatePartRequestStatus(Guid id, UpdatePartRequestStatusDto request)
    {
        var result = await partRequestRepository.UpdatePartRequestStatusAsync(id, request);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Message);
    }
}
