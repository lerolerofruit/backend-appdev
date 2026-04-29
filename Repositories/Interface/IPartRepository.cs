using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.Part;

namespace IMS_API_.Repositories.Interface;

public interface IPartRepository
{
    Task<AuthOperationResult<PartDto>> CreatePartAsync(CreatePartDto request);
    Task<AuthOperationResult<PartDto>> UpdatePartAsync(Guid partId, UpdatePartDto request);
    Task<AuthOperationResult<string>> DeletePartAsync(Guid partId);
    Task<AuthOperationResult<string>> UpdatePartStatusAsync(Guid partId, bool isActive);
    Task<AuthOperationResult<PartDto>> GetPartByIdAsync(Guid partId);
    Task<IReadOnlyCollection<PartDto>> GetPartsAsync(bool includeInactive = false);
}
