using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/staff/customers")]
[Authorize(Roles = "Staff")]
public class StaffCustomersController(IStaffCustomerRepository staffCustomerRepository) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q)
    {
        var customers = await staffCustomerRepository.SearchCustomersAsync(q);
        return Ok(customers);
    }

    [HttpGet("{customerId:guid}")]
    public async Task<IActionResult> GetCustomerDetails(Guid customerId)
    {
        var result = await staffCustomerRepository.GetCustomerDetailsAsync(customerId);
        return result.Succeeded ? Ok(result.Data) : NotFound(result.Message);
    }
}
