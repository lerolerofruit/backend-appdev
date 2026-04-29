using IMS_API_.Models.DTO.Auth;

namespace IMS_API_.Repositories.Interface;

public interface IAuthRepository
{
    Task<AuthOperationResult<AuthResponseDto>> RegisterCustomerAsync(RegisterRequestDto request);
    Task<AuthOperationResult<AuthResponseDto>> RegisterStaffAsync(RegisterRequestDto request);
    Task<AuthOperationResult<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<IReadOnlyCollection<StaffSummaryDto>> GetStaffAsync();
    Task<AuthOperationResult<string>> UpdateStaffStatusAsync(Guid staffId, bool isActive);
    Task<AuthOperationResult<string>> UpdateStaffRoleAsync(Guid staffId, string role);
}
