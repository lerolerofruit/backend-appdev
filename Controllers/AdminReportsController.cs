using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminReportsController(IAdminReportRepository adminReportRepository) : ControllerBase
{
    [HttpGet("financial")]
    public async Task<IActionResult> GetFinancialReport([FromQuery] string period = "monthly")
    {
        var result = await adminReportRepository.GetFinancialReportAsync(period);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result.Data);
    }
}
