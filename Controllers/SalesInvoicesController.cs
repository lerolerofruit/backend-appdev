using System.Security.Claims;
using System.Text;
using IMS_API_.Models.DTO.SalesInvoice;
using IMS_API_.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API_.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Staff,Admin")]
public class SalesInvoicesController(ISalesInvoiceRepository salesInvoiceRepository, IMS_API_.Data.IMSDbContext imsDbContext, IMS_API_.Services.IEmailService emailService) : ControllerBase
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

    [HttpPost("{salesInvoiceId:guid}/send-email")]
    public async Task<IActionResult> SendSalesInvoiceEmail(Guid salesInvoiceId)
    {
        var result = await salesInvoiceRepository.GetSalesInvoiceByIdAsync(salesInvoiceId);
        if (!result.Succeeded || result.Data is null)
        {
            return NotFound("Sales invoice not found.");
        }

        var invoice = result.Data;
        if (string.IsNullOrWhiteSpace(invoice.CustomerEmail))
        {
            return BadRequest("Customer does not have an email address.");
        }

        var subject = $"Invoice {invoice.InvoiceNumber}";
        var body = BuildInvoiceEmailBody(invoice);

        try
        {
            await emailService.SendEmailAsync(invoice.CustomerEmail, subject, body);

            // persist EmailSentTo if invoice exists in DB
            var entity = await imsDbContext.SalesInvoices.FindAsync(salesInvoiceId);
            if (entity is not null)
            {
                entity.EmailSentTo = invoice.CustomerEmail;
                await imsDbContext.SaveChangesAsync();
            }

            return Ok(new { message = "Email sent." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    private static string BuildInvoiceEmailBody(IMS_API_.Models.DTO.SalesInvoice.SalesInvoiceDto invoice)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<h2>Invoice {invoice.InvoiceNumber}</h2>");
        sb.AppendLine($"<p><strong>Customer:</strong> {invoice.CustomerName}</p>");
        sb.AppendLine($"<p><strong>Date:</strong> {invoice.InvoiceDate:yyyy-MM-dd}</p>");
        sb.AppendLine("<table style='width:100%;border-collapse:collapse;'>");
        sb.AppendLine("<thead><tr><th style='border:1px solid #ddd;padding:8px;text-align:left;'>Part</th><th style='border:1px solid #ddd;padding:8px;text-align:right;'>Qty</th><th style='border:1px solid #ddd;padding:8px;text-align:right;'>Unit Price</th><th style='border:1px solid #ddd;padding:8px;text-align:right;'>Discount</th><th style='border:1px solid #ddd;padding:8px;text-align:right;'>Total</th></tr></thead>");
        sb.AppendLine("<tbody>");
        foreach (var it in invoice.Items)
        {
            sb.AppendLine($"<tr><td style='border:1px solid #ddd;padding:8px;'>{it.PartName}</td><td style='border:1px solid #ddd;padding:8px;text-align:right;'>{it.Quantity}</td><td style='border:1px solid #ddd;padding:8px;text-align:right;'>Rs. {it.UnitPrice}</td><td style='border:1px solid #ddd;padding:8px;text-align:right;'>{it.Discount}%</td><td style='border:1px solid #ddd;padding:8px;text-align:right;'>Rs. {it.LineTotal}</td></tr>");
        }
        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");
        sb.AppendLine($"<p><strong>Subtotal:</strong> Rs. {invoice.SubtotalAmount}</p>");
        sb.AppendLine($"<p><strong>Loyalty discount:</strong> Rs. {invoice.LoyaltyDiscountAmount}</p>");
        sb.AppendLine($"<h3>Total: Rs. {invoice.TotalAmount}</h3>");
        return sb.ToString();
    }
}
