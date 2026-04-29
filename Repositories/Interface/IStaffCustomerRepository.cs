using IMS_API_.Models.DTO.Auth;
using IMS_API_.Models.DTO.Staff;

namespace IMS_API_.Repositories.Interface;

public interface IStaffCustomerRepository
{
    Task<IReadOnlyCollection<CustomerSearchResultDto>> SearchCustomersAsync(string? query);
    Task<AuthOperationResult<CustomerDetailsDto>> GetCustomerDetailsAsync(Guid customerId);

    Task<IReadOnlyCollection<RegularCustomerReportDto>> GetRegularCustomersAsync(int minInvoices, int days);
    Task<IReadOnlyCollection<HighSpenderReportDto>> GetHighSpendersAsync(decimal minTotalSpend);
    Task<IReadOnlyCollection<PendingCreditReportDto>> GetPendingCreditsAsync();
}
