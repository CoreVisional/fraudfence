using FraudFence.EntityModels.Enums;
using FraudFence.EntityModels.Models;

namespace FraudFence.Web.Areas.Consumer.Models;

public class ViewAllReportViewModel
{
    public int Id { get; set; }
    public string ScamCategoryName { get; set; } = "";
    public string ReporterName { get; set; } = "";
    public string ReporterEmail { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime FirstEncounteredOn { get; set; }

    public ReportStatus? Status { get; set; } = null;

    public Boolean IsPosted { get; set; } = false;

    public PostStatus? PostStatus { get; set; } = null;
}