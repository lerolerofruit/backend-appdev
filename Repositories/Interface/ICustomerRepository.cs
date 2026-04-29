using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.Customer;

namespace IMS_API_.Repositories.Interface;

public interface ICustomerRepository
{
    Task<AuthOperationResult<string>> RegisterCustomerWithVehicleByStaffAsync(Guid staffUserId, RegisterCustomerWithVehicleDto request);
    Task<AuthOperationResult<CustomerProfileDto>> GetCustomerProfileAsync(Guid userId);
    Task<AuthOperationResult<string>> UpdateCustomerProfileAsync(Guid userId, UpdateCustomerProfileDto request);
    Task<AuthOperationResult<VehicleDto>> AddVehicleAsync(Guid userId, VehicleUpsertDto request);
    Task<AuthOperationResult<string>> UpdateVehicleAsync(Guid userId, Guid vehicleId, VehicleUpsertDto request);
    Task<IReadOnlyCollection<VehicleDto>> GetVehiclesAsync(Guid userId);
}
