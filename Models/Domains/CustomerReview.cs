namespace IMS_API_.Models.Domains;

public class CustomerReview
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public int Rating { get; set; }
    public required string Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;
}
