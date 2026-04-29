namespace IMS_API_.Models.DTO.SalesInvoice;

public class CreateSalesInvoiceDto
{
    public required string InvoiceNumber { get; set; }
    public Guid CustomerId { get; set; }
    public bool IsCreditSale { get; set; }
    public DateTime? CreditDueDate { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public List<CreateSalesInvoiceItemDto> Items { get; set; } = [];
}
