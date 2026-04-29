using IMS_API_.Data;
using IMS_API_.Models.Domains;
using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.Vendor;
using IMS_API_.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Repositories.Implementation;

public class VendorRepository(IMSDbContext imsDbContext) : IVendorRepository
{
    public async Task<AuthOperationResult<VendorDto>> CreateVendorAsync(VendorUpsertDto request)
    {
        if (await imsDbContext.Vendors.AnyAsync(x => x.Name.ToLower() == request.Name.Trim().ToLower()))
        {
            return new AuthOperationResult<VendorDto> { Succeeded = false, Message = "Vendor name already exists." };
        }

        var vendor = new Vendor
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        imsDbContext.Vendors.Add(vendor);
        await imsDbContext.SaveChangesAsync();

        return new AuthOperationResult<VendorDto>
        {
            Succeeded = true,
            Message = "Vendor created.",
            Data = MapVendor(vendor)
        };
    }

    public async Task<AuthOperationResult<VendorDto>> UpdateVendorAsync(Guid vendorId, VendorUpsertDto request)
    {
        var vendor = await imsDbContext.Vendors.FirstOrDefaultAsync(x => x.Id == vendorId);
        if (vendor is null)
        {
            return new AuthOperationResult<VendorDto> { Succeeded = false, Message = "Vendor not found." };
        }

        var normalizedName = request.Name.Trim();
        if (!vendor.Name.Equals(normalizedName, StringComparison.OrdinalIgnoreCase) &&
            await imsDbContext.Vendors.AnyAsync(x => x.Id != vendorId && x.Name.ToLower() == normalizedName.ToLower()))
        {
            return new AuthOperationResult<VendorDto> { Succeeded = false, Message = "Vendor name already exists." };
        }

        vendor.Name = normalizedName;
        vendor.ContactPerson = request.ContactPerson;
        vendor.Email = request.Email;
        vendor.PhoneNumber = request.PhoneNumber;
        vendor.Address = request.Address;

        await imsDbContext.SaveChangesAsync();

        return new AuthOperationResult<VendorDto>
        {
            Succeeded = true,
            Message = "Vendor updated.",
            Data = MapVendor(vendor)
        };
    }

    public async Task<AuthOperationResult<string>> UpdateVendorStatusAsync(Guid vendorId, bool isActive)
    {
        var vendor = await imsDbContext.Vendors.FirstOrDefaultAsync(x => x.Id == vendorId);
        if (vendor is null)
        {
            return new AuthOperationResult<string> { Succeeded = false, Message = "Vendor not found." };
        }

        vendor.IsActive = isActive;
        await imsDbContext.SaveChangesAsync();

        return new AuthOperationResult<string>
        {
            Succeeded = true,
            Message = isActive ? "Vendor activated." : "Vendor deactivated.",
            Data = vendor.Id.ToString()
        };
    }

    public async Task<AuthOperationResult<VendorDto>> GetVendorByIdAsync(Guid vendorId)
    {
        var vendor = await imsDbContext.Vendors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == vendorId);
        if (vendor is null)
        {
            return new AuthOperationResult<VendorDto> { Succeeded = false, Message = "Vendor not found." };
        }

        return new AuthOperationResult<VendorDto>
        {
            Succeeded = true,
            Message = "Vendor retrieved.",
            Data = MapVendor(vendor)
        };
    }

    public async Task<IReadOnlyCollection<VendorDto>> GetVendorsAsync(bool includeInactive = false)
    {
        var query = imsDbContext.Vendors.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.Name)
            .Select(x => new VendorDto
            {
                Id = x.Id,
                Name = x.Name,
                ContactPerson = x.ContactPerson,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Address = x.Address,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    private static VendorDto MapVendor(Vendor vendor) => new()
    {
        Id = vendor.Id,
        Name = vendor.Name,
        ContactPerson = vendor.ContactPerson,
        Email = vendor.Email,
        PhoneNumber = vendor.PhoneNumber,
        Address = vendor.Address,
        IsActive = vendor.IsActive,
        CreatedAt = vendor.CreatedAt
    };
}
