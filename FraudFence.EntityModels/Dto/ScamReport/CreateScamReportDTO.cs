using FraudFence.EntityModels.Enums;

namespace FraudFence.EntityModels.Dto.ScamReport;

public class CreateScamReportDTO
{
    public int ScamCategoryId { get; set; }
    public int? ExternalAgencyId { get; set; }
    public DateTime FirstEncounteredOn { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public string ReporterEmail { get; set; } = string.Empty;
    public string? UserId { get; set; }

    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }

    public string DynamicData { get; set; } = "{}";

    public ReportStatus Status { get; set; }
}