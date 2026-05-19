using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class NotificationsController(INotificationRepository notificationRepository) : ControllerBase
{
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockParts()
    {
        var lowStockParts = await notificationRepository.GetLowStockPartsAsync();
        return Ok(lowStockParts);
    }

    [HttpGet("overdue-credits")]
    public async Task<IActionResult> GetOverdueCredits()
    {
        var overdueCredits = await notificationRepository.GetOverdueCreditsAsync();
        return Ok(overdueCredits);
    }
}
