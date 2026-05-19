using IMS_API_.Models.DTO.Notification;

namespace IMS_API_.Repositories.Interface;

public interface INotificationRepository
{
    Task<IReadOnlyCollection<LowStockPartDto>> GetLowStockPartsAsync();
    Task<IReadOnlyCollection<OverdueCreditDto>> GetOverdueCreditsAsync();
}
