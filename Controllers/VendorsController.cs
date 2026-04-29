using IMS_API_.Models.DTO.Vendor;
using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class VendorsController(IVendorRepository vendorRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetVendors([FromQuery] bool includeInactive = false)
    {
        var vendors = await vendorRepository.GetVendorsAsync(includeInactive);
        return Ok(vendors);
    }

    [HttpGet("{vendorId:guid}")]
    public async Task<IActionResult> GetVendor(Guid vendorId)
    {
        var result = await vendorRepository.GetVendorByIdAsync(vendorId);
        return result.Succeeded ? Ok(result.Data) : NotFound(result.Message);
    }

    [HttpPost]
    public async Task<IActionResult> CreateVendor(VendorUpsertDto request)
    {
        var result = await vendorRepository.CreateVendorAsync(request);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Message);
    }

    [HttpPut("{vendorId:guid}")]
    public async Task<IActionResult> UpdateVendor(Guid vendorId, VendorUpsertDto request)
    {
        var result = await vendorRepository.UpdateVendorAsync(vendorId, request);
        if (!result.Succeeded)
        {
            return result.Message == "Vendor not found." ? NotFound(result.Message) : BadRequest(result.Message);
        }

        return Ok(result.Data);
    }

    [HttpPatch("{vendorId:guid}/status")]
    public async Task<IActionResult> UpdateVendorStatus(Guid vendorId, UpdateVendorStatusDto request)
    {
        var result = await vendorRepository.UpdateVendorStatusAsync(vendorId, request.IsActive);
        if (!result.Succeeded)
        {
            return result.Message == "Vendor not found." ? NotFound(result.Message) : BadRequest(result.Message);
        }

        return Ok(new { Message = result.Message });
    }
}
