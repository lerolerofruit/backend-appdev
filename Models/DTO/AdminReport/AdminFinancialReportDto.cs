namespace IMS_API_.Models.DTO.AdminReport;

public class AdminFinancialReportDto
{
    public decimal TotalSalesRevenue { get; set; }
    public decimal TotalPurchaseCost { get; set; }
    public decimal EstimatedProfit { get; set; }
    public int TotalSalesInvoices { get; set; }
    public int TotalPurchaseInvoices { get; set; }
    public decimal PaidSalesAmount { get; set; }
    public decimal PendingCreditSalesAmount { get; set; }
    public string Period { get; set; } = string.Empty;
    public DateTime ReportGeneratedAt { get; set; } = DateTime.UtcNow;
}
