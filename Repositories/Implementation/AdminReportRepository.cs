using IMS_API_.Data;
using IMS_API_.Models.Domains;
using IMS_API_.Models.DTO.AdminReport;
using IMS_API_.Models.DTO.Auth;
using IMS_API_.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Repositories.Implementation;

public class AdminReportRepository(IMSDbContext imsDbContext) : IAdminReportRepository
{
    public async Task<AuthOperationResult<AdminFinancialReportDto>> GetFinancialReportAsync(string period)
    {
        // Validate period
        var validPeriods = new[] { "daily", "monthly", "yearly" };
        if (!validPeriods.Contains(period.ToLower()))
        {
            return new AuthOperationResult<AdminFinancialReportDto>
            {
                Succeeded = false,
                Message = "Invalid period. Use 'daily', 'monthly', or 'yearly'."
            };
        }

        try
        {
            var (startDate, endDate) = GetDateRange(period.ToLower());

            // Fetch sales invoices within the period - project only required columns to avoid mapping missing DB columns
            var salesInvoices = await imsDbContext.SalesInvoices
                .Where(x => x.InvoiceDate >= startDate && x.InvoiceDate <= endDate)
                .Select(x => new { x.Id, x.TotalAmount, x.InvoiceDate, x.PaymentStatus, x.IsCreditSale })
                .AsNoTracking()
                .ToListAsync();

            // Fetch purchase invoices within the period - project only required columns
            var purchaseInvoices = await imsDbContext.PurchaseInvoices
                .Where(x => x.InvoiceDate >= startDate && x.InvoiceDate <= endDate)
                .Select(x => new { x.Id, x.TotalAmount, x.InvoiceDate })
                .AsNoTracking()
                .ToListAsync();

            // Fetch credit payments for paid/partial amounts within the period
            var creditPayments = await imsDbContext.CreditPayments
                .Where(x => x.PaidOn >= startDate && x.PaidOn <= endDate)
                .AsNoTracking()
                .ToListAsync();

            // Calculate totals
            var totalSalesRevenue = salesInvoices.Sum(x => x.TotalAmount);
            var totalPurchaseCost = purchaseInvoices.Sum(x => x.TotalAmount);
            var estimatedProfit = totalSalesRevenue - totalPurchaseCost;

            // Calculate paid and pending amounts
            var paidSalesAmount = salesInvoices
                .Where(x => x.PaymentStatus == InvoicePaymentStatus.Paid)
                .Sum(x => x.TotalAmount);

            var partiallyPaidAmount = salesInvoices
                .Where(x => x.PaymentStatus == InvoicePaymentStatus.PartiallyPaid)
                .Sum(x => x.TotalAmount);

            // Add credit payments to get actual paid amount
            var creditPaymentAmount = creditPayments.Sum(x => x.AmountPaid);
            var totalPaidAmount = paidSalesAmount + creditPaymentAmount;

            // Calculate pending/credit amounts
            var pendingCreditSalesAmount = salesInvoices
                .Where(x => x.IsCreditSale && (x.PaymentStatus == InvoicePaymentStatus.Unpaid || x.PaymentStatus == InvoicePaymentStatus.PartiallyPaid || x.PaymentStatus == InvoicePaymentStatus.Overdue))
                .Sum(x => x.TotalAmount - (creditPayments.Where(cp => cp.SalesInvoiceId == x.Id).Sum(cp => cp.AmountPaid)));

            var report = new AdminFinancialReportDto
            {
                TotalSalesRevenue = totalSalesRevenue,
                TotalPurchaseCost = totalPurchaseCost,
                EstimatedProfit = estimatedProfit,
                TotalSalesInvoices = salesInvoices.Count,
                TotalPurchaseInvoices = purchaseInvoices.Count,
                PaidSalesAmount = totalPaidAmount,
                PendingCreditSalesAmount = pendingCreditSalesAmount > 0 ? pendingCreditSalesAmount : 0,
                Period = period.ToLower(),
                ReportGeneratedAt = DateTime.UtcNow
            };

            return new AuthOperationResult<AdminFinancialReportDto>
            {
                Succeeded = true,
                Message = $"Financial report for {period.ToLower()} period generated successfully.",
                Data = report
            };
        }
        catch (Exception ex)
        {
            return new AuthOperationResult<AdminFinancialReportDto>
            {
                Succeeded = false,
                Message = $"Error generating financial report: {ex.Message}"
            };
        }
    }

    private static (DateTime startDate, DateTime endDate) GetDateRange(string period)
    {
        var now = DateTime.UtcNow;
        return period switch
        {
            "daily" => (
                new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, DateTimeKind.Utc)
            ),
            "monthly" => (
                new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59, DateTimeKind.Utc)
            ),
            "yearly" => (
                new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(now.Year, 12, 31, 23, 59, 59, DateTimeKind.Utc)
            ),
            _ => throw new ArgumentException("Invalid period")
        };
    }
}
