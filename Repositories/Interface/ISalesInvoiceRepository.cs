using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.SalesInvoice;

namespace IMS_API_.Repositories.Interface;

public interface ISalesInvoiceRepository
{
    Task<AuthOperationResult<SalesInvoiceDto>> CreateSalesInvoiceAsync(Guid staffUserId, CreateSalesInvoiceDto request);
    Task<AuthOperationResult<SalesInvoiceDto>> GetSalesInvoiceByIdAsync(Guid salesInvoiceId);
    Task<IReadOnlyCollection<SalesInvoiceDto>> GetSalesInvoicesAsync(Guid? customerId = null);
}
