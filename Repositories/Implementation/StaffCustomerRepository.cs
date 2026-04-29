using IMS_API_.Data;
using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.Staff;
using IMS_API_.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Repositories.Implementation;

public class StaffCustomerRepository(IMSDbContext imsDbContext) : IStaffCustomerRepository
{
    public async Task<IReadOnlyCollection<CustomerSearchResultDto>> SearchCustomersAsync(string? query)
    {
        query = query?.Trim();

        var baseQuery = imsDbContext.Customers
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Vehicles)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.ToLowerInvariant();

            if (Guid.TryParse(query, out var guid))
            {
                baseQuery = baseQuery.Where(x => x.Id == guid || x.UserId == guid);
            }
            else
            {
                baseQuery = baseQuery.Where(x =>
                    x.User.FullName.ToLower().Contains(q) ||
                    x.User.PhoneNumber.ToLower().Contains(q) ||
                    x.User.Email.ToLower().Contains(q) ||
                    x.Vehicles.Any(v => v.VehicleNumber.ToLower().Contains(q)));
            }
        }

        return await baseQuery
            .OrderByDescending(x => x.RegisteredAt)
            .Select(x => new CustomerSearchResultDto
            {
                CustomerId = x.Id,
                UserId = x.UserId,
                FullName = x.User.FullName,
                Email = x.User.Email,
                PhoneNumber = x.User.PhoneNumber,
                TotalSpend = x.TotalSpend,
                OutstandingCredit = x.OutstandingCredit,
                RegisteredAt = x.RegisteredAt,
                VehicleNumbers = x.Vehicles.Select(v => v.VehicleNumber).OrderBy(v => v).ToList()
            })
            .ToListAsync();
    }

    public async Task<AuthOperationResult<CustomerDetailsDto>> GetCustomerDetailsAsync(Guid customerId)
    {
        var customer = await imsDbContext.Customers
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Vehicles)
            .Include(x => x.SalesInvoices)
            .FirstOrDefaultAsync(x => x.Id == customerId);

        if (customer is null)
        {
            return new AuthOperationResult<CustomerDetailsDto> { Succeeded = false, Message = "Customer not found." };
        }

        var dto = new CustomerDetailsDto
        {
            CustomerId = customer.Id,
            UserId = customer.UserId,
            FullName = customer.User.FullName,
            Email = customer.User.Email,
            PhoneNumber = customer.User.PhoneNumber,
            RegisteredAt = customer.RegisteredAt,
            TotalSpend = customer.TotalSpend,
            OutstandingCredit = customer.OutstandingCredit,
            CreditDueDate = customer.CreditDueDate,
            Vehicles = customer.Vehicles
                .OrderBy(v => v.VehicleNumber)
                .Select(v => new Models.DTO.Customer.VehicleDto
                {
                    Id = v.Id,
                    VehicleNumber = v.VehicleNumber,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    Vin = v.Vin,
                    Mileage = v.Mileage
                })
                .ToList(),
            SalesInvoices = customer.SalesInvoices
                .OrderByDescending(i => i.InvoiceDate)
                .Select(i => new CustomerInvoiceSummaryDto
                {
                    Id = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    InvoiceDate = i.InvoiceDate,
                    TotalAmount = i.TotalAmount,
                    IsCreditSale = i.IsCreditSale,
                    PaymentStatus = i.PaymentStatus,
                    CreditDueDate = i.CreditDueDate,
                    PaidAt = i.PaidAt
                })
                .ToList()
        };

        return new AuthOperationResult<CustomerDetailsDto> { Succeeded = true, Message = "Customer retrieved.", Data = dto };
    }

    public async Task<IReadOnlyCollection<RegularCustomerReportDto>> GetRegularCustomersAsync(int minInvoices, int days)
    {
        if (minInvoices < 1) minInvoices = 1;
        if (days < 1) days = 30;

        var fromDate = DateTime.UtcNow.AddDays(-days);

        return await imsDbContext.Customers
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.SalesInvoices)
            .Select(x => new
            {
                Customer = x,
                InvoiceCount = x.SalesInvoices.Count(i => i.InvoiceDate >= fromDate),
                LastPurchaseAt = x.SalesInvoices
                    .Where(i => i.InvoiceDate >= fromDate)
                    .OrderByDescending(i => i.InvoiceDate)
                    .Select(i => (DateTime?)i.InvoiceDate)
                    .FirstOrDefault()
            })
            .Where(x => x.InvoiceCount >= minInvoices)
            .OrderByDescending(x => x.InvoiceCount)
            .ThenByDescending(x => x.Customer.TotalSpend)
            .Select(x => new RegularCustomerReportDto
            {
                CustomerId = x.Customer.Id,
                FullName = x.Customer.User.FullName,
                PhoneNumber = x.Customer.User.PhoneNumber,
                InvoiceCount = x.InvoiceCount,
                TotalSpend = x.Customer.TotalSpend,
                LastPurchaseAt = x.LastPurchaseAt
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<HighSpenderReportDto>> GetHighSpendersAsync(decimal minTotalSpend)
    {
        if (minTotalSpend < 0) minTotalSpend = 0;

        return await imsDbContext.Customers
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.TotalSpend >= minTotalSpend)
            .OrderByDescending(x => x.TotalSpend)
            .Select(x => new HighSpenderReportDto
            {
                CustomerId = x.Id,
                FullName = x.User.FullName,
                PhoneNumber = x.User.PhoneNumber,
                TotalSpend = x.TotalSpend
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<PendingCreditReportDto>> GetPendingCreditsAsync()
    {
        return await imsDbContext.Customers
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.OutstandingCredit > 0)
            .OrderByDescending(x => x.OutstandingCredit)
            .Select(x => new PendingCreditReportDto
            {
                CustomerId = x.Id,
                FullName = x.User.FullName,
                PhoneNumber = x.User.PhoneNumber,
                OutstandingCredit = x.OutstandingCredit,
                CreditDueDate = x.CreditDueDate
            })
            .ToListAsync();
    }
}
