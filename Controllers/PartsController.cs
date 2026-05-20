using IMS_API_.Models.DTO.Part;
using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartsController(IPartRepository partRepository) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetParts([FromQuery] bool includeInactive = false)
    {
        var parts = await partRepository.GetPartsAsync(includeInactive);
        return Ok(parts);
    }

    [HttpGet("{partId:guid}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetPart(Guid partId)
    {
        var result = await partRepository.GetPartByIdAsync(partId);
        return result.Succeeded ? Ok(result.Data) : NotFound(result.Message);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePart(CreatePartDto request)
    {
        var result = await partRepository.CreatePartAsync(request);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Message);
    }

    [HttpPut("{partId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePart(Guid partId, UpdatePartDto request)
    {
        var result = await partRepository.UpdatePartAsync(partId, request);
        if (!result.Succeeded)
        {
            return result.Message is "Part not found." or "Vendor not found." ? NotFound(result.Message) : BadRequest(result.Message);
        }

        return Ok(result.Data);
    }

    [HttpPatch("{partId:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePartStatus(Guid partId, UpdatePartStatusDto request)
    {
        var result = await partRepository.UpdatePartStatusAsync(partId, request.IsActive);
        if (!result.Succeeded)
        {
            return result.Message == "Part not found." ? NotFound(result.Message) : BadRequest(result.Message);
        }

        return Ok(new { Message = result.Message });
    }

    [HttpPatch("{partId:guid}/stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddStock(Guid partId, AddStockDto request)
    {
        var result = await partRepository.AddStockAsync(partId, request);
        if (!result.Succeeded)
        {
            return result.Message == "Part not found." ? NotFound(result.Message) : BadRequest(result.Message);
        }

        return Ok(result.Data);
    }

    [HttpDelete("{partId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePart(Guid partId)
    {
        var result = await partRepository.DeletePartAsync(partId);
        if (!result.Succeeded)
        {
            return result.Message == "Part not found." ? NotFound(result.Message) : BadRequest(result.Message);
        }

        return Ok(new { Message = result.Message });
    }
}
