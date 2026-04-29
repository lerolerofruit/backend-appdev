using System.Security.Claims;
using IMS_API_.Models.DTO.SalesInvoice;
using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Staff")]
public class SalesInvoicesController(ISalesInvoiceRepository salesInvoiceRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSalesInvoices([FromQuery] Guid? customerId = null)
    {
        var invoices = await salesInvoiceRepository.GetSalesInvoicesAsync(customerId);
        return Ok(invoices);
    }

    [HttpGet("{salesInvoiceId:guid}")]
    public async Task<IActionResult> GetSalesInvoice(Guid salesInvoiceId)
    {
        var result = await salesInvoiceRepository.GetSalesInvoiceByIdAsync(salesInvoiceId);
        return result.Succeeded ? Ok(result.Data) : NotFound(result.Message);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSalesInvoice(CreateSalesInvoiceDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var staffUserId))
        {
            return Unauthorized("Invalid user context.");
        }

        var result = await salesInvoiceRepository.CreateSalesInvoiceAsync(staffUserId, request);
        if (!result.Succeeded)
        {
            return result.Message is "Customer not found." or "Part not found." or "Staff user not found." ? NotFound(result.Message) : BadRequest(result.Message);
        }

        return Ok(result.Data);
    }
}
