using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Staff,Admin")]
public class CustomerReportsController(IStaffCustomerRepository staffCustomerRepository) : ControllerBase
{
    [HttpGet("regular-customers")]
    public async Task<IActionResult> GetRegularCustomers()
    {
        // days=0 means all-time regular customer activity.
        var result = await staffCustomerRepository.GetRegularCustomersAsync(minInvoices: 1, days: 0);
        return Ok(result);
    }

    [HttpGet("high-spenders")]
    public async Task<IActionResult> GetHighSpenders()
    {
        var result = await staffCustomerRepository.GetHighSpendersAsync(minTotalSpend: 0);
        return Ok(result);
    }

    [HttpGet("pending-credits")]
    public async Task<IActionResult> GetPendingCredits()
    {
        var result = await staffCustomerRepository.GetPendingCreditsAsync();
        return Ok(result);
    }
}
