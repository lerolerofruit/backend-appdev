using System.Security.Claims;
using IMS_API_.Models.DTO.CustomerReview;
using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer")]
public class CustomerReviewsController(ICustomerReviewRepository customerReviewRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateReview(CreateCustomerReviewDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var customerId))
        {
            return Unauthorized("Invalid user context.");
        }

        var result = await customerReviewRepository.CreateReviewAsync(customerId, request);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Message);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllReviews()
    {
        var reviews = await customerReviewRepository.GetAllReviewsAsync();
        return Ok(reviews);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyReviews()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var customerId))
        {
            return Unauthorized("Invalid user context.");
        }

        var reviews = await customerReviewRepository.GetMyReviewsAsync(customerId);
        return Ok(reviews);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReviewById(Guid id)
    {
        var result = await customerReviewRepository.GetReviewByIdAsync(id);
        return result.Succeeded ? Ok(result.Data) : NotFound(result.Message);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var customerId))
        {
            return Unauthorized("Invalid user context.");
        }

        var result = await customerReviewRepository.DeleteReviewAsync(id, customerId);
        return result.Succeeded ? Ok(new { message = result.Message }) : BadRequest(result.Message);
    }
}
