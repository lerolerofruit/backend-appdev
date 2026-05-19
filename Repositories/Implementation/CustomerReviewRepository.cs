using IMS_API_.Data;
using IMS_API_.Models.Domains;
using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.CustomerReview;
using IMS_API_.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Repositories.Implementation;

public class CustomerReviewRepository(IMSDbContext imsDbContext) : ICustomerReviewRepository
{
    public async Task<AuthOperationResult<CustomerReviewDto>> CreateReviewAsync(Guid customerId, CreateCustomerReviewDto request)
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
            .FirstOrDefaultAsync(x => x.Id == customerId);
        if (customer is null)
        {
            return new AuthOperationResult<CustomerReviewDto>
            {
                Succeeded = false,
                Message = "Customer not found."
            };
        }

        var review = new Models.Domains.CustomerReview
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
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

    public async Task<IReadOnlyCollection<CustomerReviewDto>> GetMyReviewsAsync(Guid customerId)
    {
        var reviews = await imsDbContext.CustomerReviews
            .Where(x => x.CustomerId == customerId)
            .Include(x => x.Customer)
            .ThenInclude(x => x.User)
            .AsNoTracking()
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

    public async Task<AuthOperationResult<CustomerReviewDto>> DeleteReviewAsync(Guid reviewId, Guid customerId)
    {
        var review = await imsDbContext.CustomerReviews.FirstOrDefaultAsync(x => x.Id == reviewId);
        if (review is null)
        {
            return new AuthOperationResult<CustomerReviewDto>
            {
                Succeeded = false,
                Message = "Review not found."
            };
        }

        if (review.CustomerId != customerId)
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
