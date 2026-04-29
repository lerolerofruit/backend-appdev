using System.Text;
using IMS_API_.Data;
using IMS_API_.Models.Domains;
using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.SalesInvoice;
using IMS_API_.Repositories.Interface;
// Email and loyalty features removed for Milestone 1
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Repositories.Implementation;

public class SalesInvoiceRepository(IMSDbContext imsDbContext) : ISalesInvoiceRepository
{
    public async Task<AuthOperationResult<SalesInvoiceDto>> CreateSalesInvoiceAsync(Guid staffUserId, CreateSalesInvoiceDto request)
    {
        var invoiceNumber = request.InvoiceNumber.Trim();
        if (await imsDbContext.SalesInvoices.AnyAsync(x => x.InvoiceNumber.ToLower() == invoiceNumber.ToLower()))
        {
            return new AuthOperationResult<SalesInvoiceDto> { Succeeded = false, Message = "Invoice number already exists." };
        }

        var staff = await imsDbContext.AppUsers.FirstOrDefaultAsync(x => x.Id == staffUserId && x.IsActive);
        if (staff is null)
        {
            return new AuthOperationResult<SalesInvoiceDto> { Succeeded = false, Message = "Staff user not found." };
        }

        var customer = await imsDbContext.Customers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == request.CustomerId);
        if (customer is null)
        {
            return new AuthOperationResult<SalesInvoiceDto> { Succeeded = false, Message = "Customer not found." };
        }

        if (request.Items.Count == 0)
        {
            return new AuthOperationResult<SalesInvoiceDto> { Succeeded = false, Message = "Invoice must contain at least one item." };
        }

        if (request.IsCreditSale && request.CreditDueDate is null)
        {
            return new AuthOperationResult<SalesInvoiceDto> { Succeeded = false, Message = "Credit due date is required for credit sales." };
        }

        await using var tx = await imsDbContext.Database.BeginTransactionAsync();

        var invoice = new Models.Domains.SalesInvoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            CustomerId = customer.Id,
            ProcessedByStaffId = staffUserId,
            InvoiceDate = request.InvoiceDate ?? DateTime.UtcNow,
            TotalAmount = 0m,
            IsCreditSale = request.IsCreditSale,
            PaymentStatus = request.IsCreditSale ? InvoicePaymentStatus.Unpaid : InvoicePaymentStatus.Paid,
            CreditDueDate = request.IsCreditSale ? request.CreditDueDate : null,
            PaidAt = request.IsCreditSale ? null : DateTime.UtcNow
        };

        var staged = new List<(VehiclePart Part, int Quantity, decimal ManualDiscount, decimal UnitPrice)>();
        decimal subtotalBeforeLoyalty = 0m;

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                await tx.RollbackAsync();
                return new AuthOperationResult<SalesInvoiceDto> { Succeeded = false, Message = "Item quantity must be greater than zero." };
            }

            if (item.Discount < 0)
            {
                await tx.RollbackAsync();
                return new AuthOperationResult<SalesInvoiceDto> { Succeeded = false, Message = "Item discount cannot be negative." };
            }

            var part = await imsDbContext.VehicleParts.FirstOrDefaultAsync(x => x.Id == item.VehiclePartId && x.IsActive);
            if (part is null)
            {
                await tx.RollbackAsync();
                return new AuthOperationResult<SalesInvoiceDto> { Succeeded = false, Message = "Part not found." };
            }

            if (part.StockQuantity < item.Quantity)
            {
                await tx.RollbackAsync();
                return new AuthOperationResult<SalesInvoiceDto> { Succeeded = false, Message = $"Insufficient stock for part {part.PartNumber}." };
            }

            var unitPrice = part.UnitPrice;
            var manualDiscount = item.Discount;
            if (manualDiscount > unitPrice)
            {
                await tx.RollbackAsync();
                return new AuthOperationResult<SalesInvoiceDto> { Succeeded = false, Message = "Discount cannot exceed unit price." };
            }

            subtotalBeforeLoyalty += (unitPrice - manualDiscount) * item.Quantity;
            staged.Add((part, item.Quantity, manualDiscount, unitPrice));
        }

        // Loyalty program removed for Milestone 1 — only manual discounts apply.

        var items = new List<SalesInvoiceItem>();
        decimal total = 0m;

        foreach (var (part, quantity, manualDiscount, unitPrice) in staged)
        {
            var combinedDiscount = manualDiscount;
            if (combinedDiscount > unitPrice)
            {
                combinedDiscount = unitPrice;
            }

            var lineTotal = (unitPrice - combinedDiscount) * quantity;
            total += lineTotal;

            items.Add(new SalesInvoiceItem
            {
                Id = Guid.NewGuid(),
                SalesInvoiceId = invoice.Id,
                VehiclePartId = part.Id,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Discount = combinedDiscount,
                LineTotal = lineTotal
            });

            part.StockQuantity -= quantity;
        }

        invoice.TotalAmount = total;
        invoice.Items = items;

        if (invoice.IsCreditSale)
        {
            customer.OutstandingCredit += total;
            customer.CreditDueDate = invoice.CreditDueDate;
        }
        else
        {
            customer.TotalSpend += total;
        }

        imsDbContext.SalesInvoices.Add(invoice);
        await imsDbContext.SaveChangesAsync();
        await tx.CommitAsync();

        var created = await GetSalesInvoiceByIdAsync(invoice.Id);
        if (!created.Succeeded || created.Data is null)
        {
            return new AuthOperationResult<SalesInvoiceDto> { Succeeded = false, Message = "Sales invoice created, but could not be retrieved." };
        }

        return new AuthOperationResult<SalesInvoiceDto> { Succeeded = true, Message = "Sales invoice created.", Data = created.Data };
    }

    public async Task<AuthOperationResult<SalesInvoiceDto>> GetSalesInvoiceByIdAsync(Guid salesInvoiceId)
    {
        var invoice = await imsDbContext.SalesInvoices
            .AsNoTracking()
            .Include(x => x.Customer)
            .ThenInclude(c => c.User)
            .Include(x => x.Items)
            .ThenInclude(i => i.VehiclePart)
            .FirstOrDefaultAsync(x => x.Id == salesInvoiceId);

        if (invoice is null)
        {
            return new AuthOperationResult<SalesInvoiceDto> { Succeeded = false, Message = "Sales invoice not found." };
        }

        return new AuthOperationResult<SalesInvoiceDto>
        {
            Succeeded = true,
            Message = "Sales invoice retrieved.",
            Data = MapInvoice(invoice)
        };
    }

    public async Task<IReadOnlyCollection<SalesInvoiceDto>> GetSalesInvoicesAsync(Guid? customerId = null)
    {
        var query = imsDbContext.SalesInvoices
            .AsNoTracking()
            .Include(x => x.Customer)
            .ThenInclude(c => c.User)
            .Include(x => x.Items)
            .ThenInclude(i => i.VehiclePart)
            .AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId);
        }

        var invoices = await query
            .OrderByDescending(x => x.InvoiceDate)
            .ToListAsync();

        return invoices.Select(MapInvoice).ToList();
    }


    private static SalesInvoiceDto MapInvoice(Models.Domains.SalesInvoice invoice) => new()
    {
        Id = invoice.Id,
        InvoiceNumber = invoice.InvoiceNumber,
        CustomerId = invoice.CustomerId,
        CustomerName = invoice.Customer.User.FullName,
        CustomerEmail = invoice.Customer.User.Email,
        ProcessedByStaffId = invoice.ProcessedByStaffId,
        InvoiceDate = invoice.InvoiceDate,
        TotalAmount = invoice.TotalAmount,
        IsCreditSale = invoice.IsCreditSale,
        PaymentStatus = invoice.PaymentStatus,
        CreditDueDate = invoice.CreditDueDate,
        PaidAt = invoice.PaidAt,
        EmailSentTo = invoice.EmailSentTo,
        Items = invoice.Items.Select(i => new SalesInvoiceItemDto
        {
            Id = i.Id,
            VehiclePartId = i.VehiclePartId,
            PartNumber = i.VehiclePart.PartNumber,
            PartName = i.VehiclePart.Name,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Discount = i.Discount,
            LineTotal = i.LineTotal
        }).ToList()
    };

    // Email sending and email body builder removed.
}
