using IMS_API_.Data;
using IMS_API_.Models.DTO.Notification;
using IMS_API_.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Repositories.Implementation;

public class NotificationRepository(IMSDbContext imsDbContext) : INotificationRepository
{
    public async Task<IReadOnlyCollection<LowStockPartDto>> GetLowStockPartsAsync()
    {
        var lowStockParts = await imsDbContext.VehicleParts
            .Where(p => p.IsActive && p.StockQuantity < 10)
            .OrderBy(p => p.StockQuantity)
            .Select(p => new LowStockPartDto
            {
                PartId = p.Id,
                PartNumber = p.PartNumber,
                PartName = p.Name,
                CurrentStockQuantity = p.StockQuantity,
                ReorderLevel = p.ReorderLevel,
                SellingPrice = p.UnitPrice
            })
            .ToListAsync();

        return lowStockParts.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<OverdueCreditDto>> GetOverdueCreditsAsync()
    {
        var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

        var overdueInvoices = await imsDbContext.SalesInvoices
            .Include(s => s.Customer)
            .ThenInclude(c => c.User)
            .Include(s => s.CreditPayments)
            .Where(s => 
                (s.PaymentStatus == Models.Domains.InvoicePaymentStatus.Unpaid ||
                 s.PaymentStatus == Models.Domains.InvoicePaymentStatus.PartiallyPaid ||
                 s.PaymentStatus == Models.Domains.InvoicePaymentStatus.Overdue) &&
                s.InvoiceDate <= oneMonthAgo)
            .OrderByDescending(s => s.InvoiceDate)
            .Select(s => new OverdueCreditDto
            {
                CustomerId = s.Customer.Id,
                CustomerName = s.Customer.User.FullName,
                CustomerEmail = s.Customer.User.Email,
                CustomerPhone = s.Customer.User.PhoneNumber,
                InvoiceId = s.Id,
                InvoiceNumber = s.InvoiceNumber,
                InvoiceDate = s.InvoiceDate,
                DueAmount = s.TotalAmount - s.CreditPayments.Sum(cp => cp.AmountPaid),
                TotalAmount = s.TotalAmount,
                CreditDueDate = s.CreditDueDate,
                DaysOverdue = (int)(DateTime.UtcNow - s.InvoiceDate).TotalDays
            })
            .ToListAsync();

        return overdueInvoices.AsReadOnly();
    }
}
