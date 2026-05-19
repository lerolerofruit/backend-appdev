namespace IMS_API_.Models.DTO.PartRequest;

public class CreatePartRequestDto
{
    public required string RequestedPartName { get; set; }
    public string? RequestedPartNumber { get; set; }
    public int Quantity { get; set; } = 1;
}
