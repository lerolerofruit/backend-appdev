using IMS_API_.Data;
using IMS_API_.Models.Domains;
using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.CustomerReview;
using IMS_API_.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Repositories.Implementation;

public class CustomerReviewRepository(IMSDbContext imsDbContext) : ICustomerReviewRepository
{
    public async Task<AuthOperationResult<CustomerReviewDto>> CreateReviewAsync(Guid customerUserId, CreateCustomerReviewDto request)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            return new AuthOperationResult<CustomerReviewDto>
            {
                Succeeded = false,
                Message = "Rating must be between 1 and 5."
            };
        }

        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            return new AuthOperationResult<CustomerReviewDto>
            {
                Succeeded = false,
                Message = "Comment is required."
            };
        }

        var customer = await imsDbContext.Customers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == customerUserId);
        if (customer is null)
        {
            return new AuthOperationResult<CustomerReviewDto>
            {
                Succeeded = false,
                Message = "Customer not found."
            };
        }

        var hasCompletedService = await imsDbContext.ServiceAppointments
            .AsNoTracking()
            .AnyAsync(x => x.CustomerId == customer.Id && x.Status == AppointmentStatus.Completed);

        if (!hasCompletedService)
        {
            return new AuthOperationResult<CustomerReviewDto>
            {
                Succeeded = false,
                Message = "You can post a review after completing at least one service appointment."
            };
        }

        var review = new Models.Domains.CustomerReview
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Rating = request.Rating,
            Comment = request.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await imsDbContext.CustomerReviews.AddAsync(review);
        await imsDbContext.SaveChangesAsync();

        var dto = new CustomerReviewDto
        {
            Id = review.Id,
            CustomerId = review.CustomerId,
            CustomerName = customer.User?.FullName,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        return new AuthOperationResult<CustomerReviewDto>
        {
            Succeeded = true,
            Message = "Review created successfully.",
            Data = dto
        };
    }

    public async Task<IReadOnlyCollection<CustomerReviewDto>> GetMyReviewsAsync(Guid customerUserId)
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

        var reviews = await imsDbContext.CustomerReviews
            .Where(x => x.CustomerId == customerId.Value)
            .Include(x => x.Customer)
            .ThenInclude(x => x.User)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return reviews.Select(x => new CustomerReviewDto
        {
            Id = x.Id,
            CustomerId = x.CustomerId,
            CustomerName = x.Customer.User?.FullName,
            Rating = x.Rating,
            Comment = x.Comment,
            CreatedAt = x.CreatedAt
        }).ToList();
    }

    public async Task<IReadOnlyCollection<CustomerReviewDto>> GetAllReviewsAsync()
    {
        var reviews = await imsDbContext.CustomerReviews
            .Include(x => x.Customer)
            .ThenInclude(x => x.User)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return reviews.Select(x => new CustomerReviewDto
        {
            Id = x.Id,
            CustomerId = x.CustomerId,
            CustomerName = x.Customer.User?.FullName,
            Rating = x.Rating,
            Comment = x.Comment,
            CreatedAt = x.CreatedAt
        }).ToList();
    }

    public async Task<AuthOperationResult<CustomerReviewDto>> GetReviewByIdAsync(Guid reviewId)
    {
        var review = await imsDbContext.CustomerReviews
            .Include(x => x.Customer)
            .ThenInclude(x => x.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == reviewId);

        if (review is null)
        {
            return new AuthOperationResult<CustomerReviewDto>
            {
                Succeeded = false,
                Message = "Review not found."
            };
        }

        var dto = new CustomerReviewDto
        {
            Id = review.Id,
            CustomerId = review.CustomerId,
            CustomerName = review.Customer.User?.FullName,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        return new AuthOperationResult<CustomerReviewDto>
        {
            Succeeded = true,
            Data = dto
        };
    }

    public async Task<AuthOperationResult<CustomerReviewDto>> DeleteReviewAsync(Guid reviewId, Guid customerUserId)
    {
        var customerId = await imsDbContext.Customers
            .AsNoTracking()
            .Where(x => x.UserId == customerUserId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!customerId.HasValue)
        {
            return new AuthOperationResult<CustomerReviewDto>
            {
                Succeeded = false,
                Message = "Customer not found."
            };
        }

        var review = await imsDbContext.CustomerReviews.FirstOrDefaultAsync(x => x.Id == reviewId);
        if (review is null)
        {
            return new AuthOperationResult<CustomerReviewDto>
            {
                Succeeded = false,
                Message = "Review not found."
            };
        }

        if (review.CustomerId != customerId.Value)
        {
            return new AuthOperationResult<CustomerReviewDto>
            {
                Succeeded = false,
                Message = "You can only delete your own reviews."
            };
        }

        imsDbContext.CustomerReviews.Remove(review);
        await imsDbContext.SaveChangesAsync();

        return new AuthOperationResult<CustomerReviewDto>
        {
            Succeeded = true,
            Message = "Review deleted successfully."
        };
    }
}
