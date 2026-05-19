using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.PartRequest;

namespace IMS_API_.Repositories.Interface;

public interface IPartRequestRepository
{
    Task<AuthOperationResult<PartRequestDto>> CreatePartRequestAsync(Guid customerUserId, CreatePartRequestDto request);
    Task<IReadOnlyCollection<PartRequestDto>> GetMyPartRequestsAsync(Guid customerUserId);
    Task<IReadOnlyCollection<PartRequestDto>> GetAllPartRequestsAsync();
    Task<AuthOperationResult<PartRequestDto>> GetPartRequestByIdAsync(Guid partRequestId);
    Task<AuthOperationResult<PartRequestDto>> UpdatePartRequestStatusAsync(Guid partRequestId, UpdatePartRequestStatusDto request);
}
