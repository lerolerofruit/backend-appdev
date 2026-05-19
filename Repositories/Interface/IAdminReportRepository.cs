using IMS_API_.Models.DTO.AdminReport;
using IMS_API_.Models.DTO.Auth;

namespace IMS_API_.Repositories.Interface;

public interface IAdminReportRepository
{
    Task<AuthOperationResult<AdminFinancialReportDto>> GetFinancialReportAsync(string period);
}
