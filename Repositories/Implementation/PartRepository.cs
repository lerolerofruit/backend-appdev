using IMS_API_.Data;
using IMS_API_.Models.Domains;
using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.Part;
using IMS_API_.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Repositories.Implementation;

public class PartRepository(IMSDbContext imsDbContext) : IPartRepository
{
    public async Task<AuthOperationResult<PartDto>> CreatePartAsync(CreatePartDto request)
    {
        var partNumber = request.PartNumber.Trim();
        if (await imsDbContext.VehicleParts.AnyAsync(x => x.PartNumber.ToLower() == partNumber.ToLower()))
        {
            return new AuthOperationResult<PartDto> { Succeeded = false, Message = "Part number already exists." };
        }

        if (request.PrimaryVendorId.HasValue)
        {
            var vendorExists = await imsDbContext.Vendors.AnyAsync(x => x.Id == request.PrimaryVendorId && x.IsActive);
            if (!vendorExists)
            {
                return new AuthOperationResult<PartDto> { Succeeded = false, Message = "Vendor not found." };
            }
        }

        var part = new VehiclePart
        {
            Id = Guid.NewGuid(),
            PartNumber = partNumber,
            Name = request.Name.Trim(),
            Description = request.Description,
            PrimaryVendorId = request.PrimaryVendorId,
            UnitPrice = request.UnitPrice,
            StockQuantity = request.InitialStockQuantity,
            ReorderLevel = request.ReorderLevel,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        imsDbContext.VehicleParts.Add(part);
        await imsDbContext.SaveChangesAsync();

        return new AuthOperationResult<PartDto>
        {
            Succeeded = true,
            Message = "Part created.",
            Data = await MapPartAsync(part.Id)
        };
    }

    public async Task<AuthOperationResult<PartDto>> UpdatePartAsync(Guid partId, UpdatePartDto request)
    {
        var part = await imsDbContext.VehicleParts.FirstOrDefaultAsync(x => x.Id == partId);
        if (part is null)
        {
            return new AuthOperationResult<PartDto> { Succeeded = false, Message = "Part not found." };
        }

        var partNumber = request.PartNumber.Trim();
        if (!part.PartNumber.Equals(partNumber, StringComparison.OrdinalIgnoreCase) &&
            await imsDbContext.VehicleParts.AnyAsync(x => x.Id != partId && x.PartNumber.ToLower() == partNumber.ToLower()))
        {
            return new AuthOperationResult<PartDto> { Succeeded = false, Message = "Part number already exists." };
        }

        if (request.PrimaryVendorId.HasValue)
        {
            var vendorExists = await imsDbContext.Vendors.AnyAsync(x => x.Id == request.PrimaryVendorId && x.IsActive);
            if (!vendorExists)
            {
                return new AuthOperationResult<PartDto> { Succeeded = false, Message = "Vendor not found." };
            }
        }

        part.PartNumber = partNumber;
        part.Name = request.Name.Trim();
        part.Description = request.Description;
        part.PrimaryVendorId = request.PrimaryVendorId;
        part.UnitPrice = request.UnitPrice;
        part.ReorderLevel = request.ReorderLevel;

        await imsDbContext.SaveChangesAsync();

        return new AuthOperationResult<PartDto>
        {
            Succeeded = true,
            Message = "Part updated.",
            Data = await MapPartAsync(partId)
        };
    }

    public async Task<AuthOperationResult<string>> DeletePartAsync(Guid partId)
    {
        var part = await imsDbContext.VehicleParts.FirstOrDefaultAsync(x => x.Id == partId);
        if (part is null)
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Part not found." };
        }

        // Soft delete
        part.IsActive = false;
        await imsDbContext.SaveChangesAsync();

        return new AuthOperationResult<string>
        {
            Succeeded = true,
            Message = "Part deleted.",
            Data = part.Id.ToString()
        };
    }

    public async Task<AuthOperationResult<string>> UpdatePartStatusAsync(Guid partId, bool isActive)
    {
        var part = await imsDbContext.VehicleParts.FirstOrDefaultAsync(x => x.Id == partId);
        if (part is null)
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Part not found." };
        }

        part.IsActive = isActive;
        await imsDbContext.SaveChangesAsync();

        return new AuthOperationResult<string>
        {
            Succeeded = true,
            Message = isActive ? "Part activated." : "Part deactivated.",
            Data = part.Id.ToString()
        };
    }

    public async Task<AuthOperationResult<PartDto>> GetPartByIdAsync(Guid partId)
    {
        var dto = await imsDbContext.VehicleParts
            .AsNoTracking()
            .Include(x => x.PrimaryVendor)
            .Where(x => x.Id == partId)
            .Select(x => new PartDto
            {
                Id = x.Id,
                PartNumber = x.PartNumber,
                Name = x.Name,
                Description = x.Description,
                PrimaryVendorId = x.PrimaryVendorId,
                PrimaryVendorName = x.PrimaryVendor != null ? x.PrimaryVendor.Name : null,
                UnitPrice = x.UnitPrice,
                StockQuantity = x.StockQuantity,
                ReorderLevel = x.ReorderLevel,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (dto is null)
        {
            return new AuthOperationResult<PartDto> { Succeeded = false, Message = "Part not found." };
        }

        return new AuthOperationResult<PartDto> { Succeeded = true, Message = "Part retrieved.", Data = dto };
    }

    public async Task<IReadOnlyCollection<PartDto>> GetPartsAsync(bool includeInactive = false)
    {
        var query = imsDbContext.VehicleParts
            .AsNoTracking()
            .Include(x => x.PrimaryVendor)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.Name)
            .Select(x => new PartDto
            {
                Id = x.Id,
                PartNumber = x.PartNumber,
                Name = x.Name,
                Description = x.Description,
                PrimaryVendorId = x.PrimaryVendorId,
                PrimaryVendorName = x.PrimaryVendor != null ? x.PrimaryVendor.Name : null,
                UnitPrice = x.UnitPrice,
                StockQuantity = x.StockQuantity,
                ReorderLevel = x.ReorderLevel,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    private async Task<PartDto> MapPartAsync(Guid partId)
        => await imsDbContext.VehicleParts
            .AsNoTracking()
            .Include(x => x.PrimaryVendor)
            .Where(x => x.Id == partId)
            .Select(x => new PartDto
            {
                Id = x.Id,
                PartNumber = x.PartNumber,
                Name = x.Name,
                Description = x.Description,
                PrimaryVendorId = x.PrimaryVendorId,
                PrimaryVendorName = x.PrimaryVendor != null ? x.PrimaryVendor.Name : null,
                UnitPrice = x.UnitPrice,
                StockQuantity = x.StockQuantity,
                ReorderLevel = x.ReorderLevel,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .FirstAsync();
}
