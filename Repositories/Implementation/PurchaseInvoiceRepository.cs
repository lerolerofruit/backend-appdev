using IMS_API_.Data;
using IMS_API_.Models.Domains;
using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.PurchaseInvoice;
using IMS_API_.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Repositories.Implementation;

public class PurchaseInvoiceRepository(IMSDbContext imsDbContext) : IPurchaseInvoiceRepository
{
    public async Task<AuthOperationResult<PurchaseInvoiceDto>> CreatePurchaseInvoiceAsync(Guid adminUserId, CreatePurchaseInvoiceDto request)
    {
        var invoiceNumber = request.InvoiceNumber.Trim();
        if (await imsDbContext.PurchaseInvoices.AnyAsync(x => x.InvoiceNumber.ToLower() == invoiceNumber.ToLower()))
        {
            return new AuthOperationResult<PurchaseInvoiceDto> { Succeeded = false, Message = "Invoice number already exists." };
        }

        var vendor = await imsDbContext.Vendors.FirstOrDefaultAsync(x => x.Id == request.VendorId && x.IsActive);
        if (vendor is null)
        {
            return new AuthOperationResult<PurchaseInvoiceDto> { Succeeded = false, Message = "Vendor not found." };
        }

        if (request.Items.Count == 0)
        {
            return new AuthOperationResult<PurchaseInvoiceDto> { Succeeded = false, Message = "Invoice must contain at least one item." };
        }

        var adminExists = await imsDbContext.AppUsers.AnyAsync(x => x.Id == adminUserId && x.IsActive);
        if (!adminExists)
        {
            return new AuthOperationResult<PurchaseInvoiceDto> { Succeeded = false, Message = "Admin user not found." };
        }

        await using var tx = await imsDbContext.Database.BeginTransactionAsync();

            var invoiceDate = request.InvoiceDate.HasValue
                ? DateTime.SpecifyKind(request.InvoiceDate.Value, DateTimeKind.Utc)
                : DateTime.UtcNow;

            var invoice = new PurchaseInvoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = invoiceNumber,
                VendorId = vendor.Id,
                CreatedByAdminId = adminUserId,
                InvoiceDate = invoiceDate,
                TotalAmount = 0m
            };

        var items = new List<PurchaseInvoiceItem>();
        decimal total = 0m;

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                await tx.RollbackAsync();
                return new AuthOperationResult<PurchaseInvoiceDto> { Succeeded = false, Message = "Item quantity must be greater than zero." };
            }

            if (item.UnitCost < 0)
            {
                await tx.RollbackAsync();
                return new AuthOperationResult<PurchaseInvoiceDto> { Succeeded = false, Message = "Item unit cost cannot be negative." };
            }

            var part = await imsDbContext.VehicleParts.FirstOrDefaultAsync(x => x.Id == item.VehiclePartId && x.IsActive);
            if (part is null)
            {
                await tx.RollbackAsync();
                return new AuthOperationResult<PurchaseInvoiceDto> { Succeeded = false, Message = "Part not found." };
            }

            var lineTotal = item.UnitCost * item.Quantity;
            total += lineTotal;

            items.Add(new PurchaseInvoiceItem
            {
                Id = Guid.NewGuid(),
                PurchaseInvoiceId = invoice.Id,
                VehiclePartId = part.Id,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost,
                LineTotal = lineTotal
            });

            part.StockQuantity += item.Quantity;
        }

        invoice.TotalAmount = total;
        invoice.Items = items;

        imsDbContext.PurchaseInvoices.Add(invoice);
        await imsDbContext.SaveChangesAsync();
        await tx.CommitAsync();

        var created = await GetPurchaseInvoiceByIdAsync(invoice.Id);
        if (!created.Succeeded || created.Data is null)
        {
            return new AuthOperationResult<PurchaseInvoiceDto>
            {
                Succeeded = false,
                Message = "Purchase invoice created, but could not be retrieved."
            };
        }

        return new AuthOperationResult<PurchaseInvoiceDto>
        {
            Succeeded = true,
            Message = "Purchase invoice created.",
            Data = created.Data
        };
    }

    public async Task<AuthOperationResult<PurchaseInvoiceDto>> GetPurchaseInvoiceByIdAsync(Guid purchaseInvoiceId)
    {
        var invoice = await imsDbContext.PurchaseInvoices
            .AsNoTracking()
            .Include(x => x.Vendor)
            .Include(x => x.Items)
            .ThenInclude(i => i.VehiclePart)
            .FirstOrDefaultAsync(x => x.Id == purchaseInvoiceId);

        if (invoice is null)
        {
            return new AuthOperationResult<PurchaseInvoiceDto> { Succeeded = false, Message = "Purchase invoice not found." };
        }

        return new AuthOperationResult<PurchaseInvoiceDto>
        {
            Succeeded = true,
            Message = "Purchase invoice retrieved.",
            Data = new PurchaseInvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                VendorId = invoice.VendorId,
                VendorName = invoice.Vendor.Name,
                CreatedByAdminId = invoice.CreatedByAdminId,
                InvoiceDate = invoice.InvoiceDate,
                TotalAmount = invoice.TotalAmount,
                Items = invoice.Items
                    .Select(i => new PurchaseInvoiceItemDto
                    {
                        Id = i.Id,
                        VehiclePartId = i.VehiclePartId,
                        PartNumber = i.VehiclePart.PartNumber,
                        PartName = i.VehiclePart.Name,
                        Quantity = i.Quantity,
                        UnitCost = i.UnitCost,
                        LineTotal = i.LineTotal
                    })
                    .ToList()
            }
        };
    }

    public async Task<IReadOnlyCollection<PurchaseInvoiceDto>> GetPurchaseInvoicesAsync()
    {
        return await imsDbContext.PurchaseInvoices
            .AsNoTracking()
            .Include(x => x.Vendor)
            .Include(x => x.Items)
            .ThenInclude(i => i.VehiclePart)
            .OrderByDescending(x => x.InvoiceDate)
            .Select(x => new PurchaseInvoiceDto
            {
                Id = x.Id,
                InvoiceNumber = x.InvoiceNumber,
                VendorId = x.VendorId,
                VendorName = x.Vendor.Name,
                CreatedByAdminId = x.CreatedByAdminId,
                InvoiceDate = x.InvoiceDate,
                TotalAmount = x.TotalAmount,
                Items = x.Items
                    .Select(i => new PurchaseInvoiceItemDto
                    {
                        Id = i.Id,
                        VehiclePartId = i.VehiclePartId,
                        PartNumber = i.VehiclePart.PartNumber,
                        PartName = i.VehiclePart.Name,
                        Quantity = i.Quantity,
                        UnitCost = i.UnitCost,
                        LineTotal = i.LineTotal
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<AuthOperationResult<bool>> DeletePurchaseInvoiceAsync(Guid purchaseInvoiceId)
    {
        var invoice = await imsDbContext.PurchaseInvoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == purchaseInvoiceId);

        if (invoice is null)
        {
            return new AuthOperationResult<bool> { Succeeded = false, Message = "Purchase invoice not found." };
        }

        await using var tx = await imsDbContext.Database.BeginTransactionAsync();
        try
        {
            // revert stock quantities
            foreach (var item in invoice.Items)
            {
                var part = await imsDbContext.VehicleParts.FirstOrDefaultAsync(p => p.Id == item.VehiclePartId);
                if (part is not null)
                {
                    part.StockQuantity = Math.Max(0, part.StockQuantity - item.Quantity);
                }
            }

            imsDbContext.PurchaseInvoices.Remove(invoice);
            await imsDbContext.SaveChangesAsync();
            await tx.CommitAsync();

            return new AuthOperationResult<bool> { Succeeded = true, Message = "Purchase invoice deleted.", Data = true };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new AuthOperationResult<bool> { Succeeded = false, Message = ex.Message };
        }
    }
}
