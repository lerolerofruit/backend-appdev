using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.Vendor;

namespace IMS_API_.Repositories.Interface;

public interface IVendorRepository
{
    Task<AuthOperationResult<VendorDto>> CreateVendorAsync(VendorUpsertDto request);
    Task<AuthOperationResult<VendorDto>> UpdateVendorAsync(Guid vendorId, VendorUpsertDto request);
    Task<AuthOperationResult<string>> UpdateVendorStatusAsync(Guid vendorId, bool isActive);
    Task<AuthOperationResult<VendorDto>> GetVendorByIdAsync(Guid vendorId);
    Task<IReadOnlyCollection<VendorDto>> GetVendorsAsync(bool includeInactive = false);
}
