namespace FraudFence.EntityModels.Dto.ScamReport;

public class ScamReportDTO
{
    public int ScamCategoryId { get; set; }

    public int? ExternalAgencyId { get; set; }

    public string? GuestId { get; set; }
        
    public int? UserId { get; set; }
        
    public DateTime FirstEncounteredOn { get; set; }

    public required string Description { get; set; }

    public required string ReporterName { get; set; }

    public required string ReporterEmail { get; set; }
        
    public required string DynamicData { get; set; } = "{}";
}