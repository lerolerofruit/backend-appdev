namespace IMS_API_.Models.DTO.CustomerReview;

public class CreateCustomerReviewDto
{
    public int Rating { get; set; }
    public required string Comment { get; set; }
}
