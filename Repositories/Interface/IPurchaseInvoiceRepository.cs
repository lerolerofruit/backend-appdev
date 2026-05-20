using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.PurchaseInvoice;

namespace IMS_API_.Repositories.Interface;

public interface IPurchaseInvoiceRepository
{
    Task<AuthOperationResult<PurchaseInvoiceDto>> CreatePurchaseInvoiceAsync(Guid adminUserId, CreatePurchaseInvoiceDto request);
    Task<AuthOperationResult<PurchaseInvoiceDto>> GetPurchaseInvoiceByIdAsync(Guid purchaseInvoiceId);
    Task<IReadOnlyCollection<PurchaseInvoiceDto>> GetPurchaseInvoicesAsync();
    Task<AuthOperationResult<bool>> DeletePurchaseInvoiceAsync(Guid purchaseInvoiceId);
}
