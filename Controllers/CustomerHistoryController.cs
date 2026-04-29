using System.Security.Claims;
using IMS_API_.Data;
using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/customer/history")]
[Authorize(Roles = "Customer")]
public class CustomerHistoryController(IMSDbContext imsDbContext, ISalesInvoiceRepository salesInvoiceRepository) : ControllerBase
{
    [HttpGet("sales-invoices")]
    public async Task<IActionResult> GetMySalesInvoices()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized("Invalid token.");
        }

        var customerId = await imsDbContext.Customers
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!customerId.HasValue)
        {
            return NotFound("Customer profile not found.");
        }

        var invoices = await salesInvoiceRepository.GetSalesInvoicesAsync(customerId);
        return Ok(invoices);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
