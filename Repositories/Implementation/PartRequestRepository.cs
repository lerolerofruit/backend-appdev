using IMS_API_.Data;
using IMS_API_.Models.Domains;
using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.PartRequest;
using IMS_API_.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Repositories.Implementation;

public class PartRequestRepository(IMSDbContext imsDbContext) : IPartRequestRepository
{
    private static PartRequestDto ToDto(Models.Domains.PartRequest request)
    {
        return new PartRequestDto
        {
            Id = request.Id,
            CustomerId = request.CustomerId,
            RequestedByName = request.Customer?.User?.FullName,
            RequestedByEmail = request.Customer?.User?.Email,
            RequestedPartName = request.RequestedPartName,
            RequestedPartNumber = request.RequestedPartNumber,
            Quantity = request.Quantity,
            Status = request.Status.ToString(),
            RequestedOn = request.RequestedOn,
            ResolvedOn = request.ResolvedOn
        };
    }

    public async Task<AuthOperationResult<PartRequestDto>> CreatePartRequestAsync(Guid customerUserId, CreatePartRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RequestedPartName))
        {
            return new AuthOperationResult<PartRequestDto>
            {
                Succeeded = false,
                Message = "Part name is required."
            };
        }

        if (request.Quantity <= 0)
        {
            return new AuthOperationResult<PartRequestDto>
            {
                Succeeded = false,
                Message = "Quantity must be greater than zero."
            };
        }

        var customer = await imsDbContext.Customers.FirstOrDefaultAsync(x => x.UserId == customerUserId);
        if (customer is null)
        {
            return new AuthOperationResult<PartRequestDto>
            {
                Succeeded = false,
                Message = "Customer not found."
            };
        }

        var partRequest = new Models.Domains.PartRequest
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            RequestedPartName = request.RequestedPartName.Trim(),
            RequestedPartNumber = request.RequestedPartNumber?.Trim(),
            Quantity = request.Quantity,
            Status = PartRequestStatus.Pending,
            RequestedOn = DateTime.UtcNow
        };

        await imsDbContext.PartRequests.AddAsync(partRequest);
        
        try
        {
            await imsDbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return new AuthOperationResult<PartRequestDto>
            {
                Succeeded = false,
                Message = $"Failed to save part request: {ex.InnerException?.Message ?? ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new AuthOperationResult<PartRequestDto>
            {
                Succeeded = false,
                Message = $"An error occurred while creating the part request: {ex.Message}"
            };
        }

        var createdRequest = await imsDbContext.PartRequests
            .AsNoTracking()
            .Include(x => x.Customer)
                .ThenInclude(x => x.User)
            .FirstAsync(x => x.Id == partRequest.Id);

        var dto = ToDto(createdRequest);

        return new AuthOperationResult<PartRequestDto>
        {
            Succeeded = true,
            Message = "Part request created successfully.",
            Data = dto
        };
    }

    public async Task<IReadOnlyCollection<PartRequestDto>> GetMyPartRequestsAsync(Guid customerUserId)
    {
        var customerId = await imsDbContext.Customers
            .AsNoTracking()
            .Where(x => x.UserId == customerUserId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!customerId.HasValue)
        {
            return [];
        }

        var requests = await imsDbContext.PartRequests
            .Where(x => x.CustomerId == customerId)
            .AsNoTracking()
            .Include(x => x.Customer)
                .ThenInclude(x => x.User)
            .ToListAsync();

        return requests.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyCollection<PartRequestDto>> GetAllPartRequestsAsync()
    {
        var requests = await imsDbContext.PartRequests
            .AsNoTracking()
            .Include(x => x.Customer)
                .ThenInclude(x => x.User)
            .ToListAsync();

        return requests.Select(ToDto).ToList();
    }

    public async Task<AuthOperationResult<PartRequestDto>> GetPartRequestByIdAsync(Guid partRequestId)
    {
        var request = await imsDbContext.PartRequests
            .AsNoTracking()
            .Include(x => x.Customer)
                .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == partRequestId);

        if (request is null)
        {
            return new AuthOperationResult<PartRequestDto>
            {
                Succeeded = false,
                Message = "Part request not found."
            };
        }

        var dto = ToDto(request);

        return new AuthOperationResult<PartRequestDto>
        {
            Succeeded = true,
            Data = dto
        };
    }

    public async Task<AuthOperationResult<PartRequestDto>> UpdatePartRequestStatusAsync(Guid partRequestId, UpdatePartRequestStatusDto request)
    {
        var validStatuses = new[] { "Pending", "Approved", "Rejected", "Fulfilled" };
        if (!validStatuses.Contains(request.Status))
        {
            return new AuthOperationResult<PartRequestDto>
            {
                Succeeded = false,
                Message = "Invalid status. Valid statuses are: Pending, Approved, Rejected, Fulfilled."
            };
        }

        var partRequest = await imsDbContext.PartRequests.FirstOrDefaultAsync(x => x.Id == partRequestId);
        if (partRequest is null)
        {
            return new AuthOperationResult<PartRequestDto>
            {
                Succeeded = false,
                Message = "Part request not found."
            };
        }

        if (Enum.TryParse<PartRequestStatus>(request.Status, out var status))
        {
            partRequest.Status = status;
            partRequest.ResolvedOn = DateTime.UtcNow;
            await imsDbContext.SaveChangesAsync();
        }

        var updatedRequest = await imsDbContext.PartRequests
            .AsNoTracking()
            .Include(x => x.Customer)
                .ThenInclude(x => x.User)
            .FirstAsync(x => x.Id == partRequest.Id);

        var dto = ToDto(updatedRequest);

        return new AuthOperationResult<PartRequestDto>
        {
            Succeeded = true,
            Message = "Part request status updated successfully.",
            Data = dto
        };
    }

    public async Task<AuthOperationResult<bool>> DeletePartRequestAsync(Guid partRequestId, Guid requesterUserId, bool canDeleteAny)
    {
        var partRequest = await imsDbContext.PartRequests
            .FirstOrDefaultAsync(x => x.Id == partRequestId);

        if (partRequest is null)
        {
            return new AuthOperationResult<bool>
            {
                Succeeded = false,
                Message = "Part request not found.",
                Data = false
            };
        }

        if (!canDeleteAny)
        {
            var customerId = await imsDbContext.Customers
                .AsNoTracking()
                .Where(x => x.UserId == requesterUserId)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync();

            if (!customerId.HasValue || customerId.Value != partRequest.CustomerId)
            {
                return new AuthOperationResult<bool>
                {
                    Succeeded = false,
                    Message = "You are not allowed to remove this part request.",
                    Data = false
                };
            }
        }

        imsDbContext.PartRequests.Remove(partRequest);
        await imsDbContext.SaveChangesAsync();

        return new AuthOperationResult<bool>
        {
            Succeeded = true,
            Message = "Part request removed successfully.",
            Data = true
        };
    }
}
