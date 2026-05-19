using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.CustomerReview;

namespace IMS_API_.Repositories.Interface;

public interface ICustomerReviewRepository
{
    Task<AuthOperationResult<CustomerReviewDto>> CreateReviewAsync(Guid customerId, CreateCustomerReviewDto request);
    Task<IReadOnlyCollection<CustomerReviewDto>> GetMyReviewsAsync(Guid customerId);
    Task<IReadOnlyCollection<CustomerReviewDto>> GetAllReviewsAsync();
    Task<AuthOperationResult<CustomerReviewDto>> GetReviewByIdAsync(Guid reviewId);
    Task<AuthOperationResult<CustomerReviewDto>> DeleteReviewAsync(Guid reviewId, Guid customerId);
}
