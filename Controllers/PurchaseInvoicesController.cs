using System.Security.Claims;
using IMS_API_.Models.DTO.PurchaseInvoice;
using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PurchaseInvoicesController(IPurchaseInvoiceRepository purchaseInvoiceRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPurchaseInvoices()
    {
        var invoices = await purchaseInvoiceRepository.GetPurchaseInvoicesAsync();
        return Ok(invoices);
    }

    [HttpGet("{purchaseInvoiceId:guid}")]
    public async Task<IActionResult> GetPurchaseInvoice(Guid purchaseInvoiceId)
    {
        var result = await purchaseInvoiceRepository.GetPurchaseInvoiceByIdAsync(purchaseInvoiceId);
        return result.Succeeded ? Ok(result.Data) : NotFound(result.Message);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePurchaseInvoice(CreatePurchaseInvoiceDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var adminUserId))
        {
            return Unauthorized("Invalid user context.");
        }

        var result = await purchaseInvoiceRepository.CreatePurchaseInvoiceAsync(adminUserId, request);
        if (!result.Succeeded)
        {
            return result.Message is "Vendor not found." or "Part not found." or "Admin user not found." ? NotFound(result.Message) : BadRequest(result.Message);
        }

        return Ok(result.Data);
    }

    [HttpDelete("{purchaseInvoiceId:guid}")]
    public async Task<IActionResult> DeletePurchaseInvoice(Guid purchaseInvoiceId)
    {
        var result = await purchaseInvoiceRepository.DeletePurchaseInvoiceAsync(purchaseInvoiceId);
        if (!result.Succeeded)
        {
            return result.Message == "Purchase invoice not found." ? NotFound(result.Message) : BadRequest(result.Message);
        }

        return Ok(new { message = "Purchase invoice deleted." });
    }
}
