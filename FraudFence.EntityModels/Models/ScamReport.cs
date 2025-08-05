using FraudFence.EntityModels.common;
using FraudFence.EntityModels.Enums;

namespace FraudFence.EntityModels.Models
{
    public class ScamReport : BaseEntity
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

        public ReportStatus Status { get; set; } = ReportStatus.Submitted;
        
        public virtual ApplicationUser? User { get; set; }

        public virtual ExternalAgency ExternalAgency { get; set; } = null!;

        public virtual ScamCategory ScamCategory { get; set; } = null!;

        public virtual ICollection<ScamReportAttachment> ScamReportAttachments { get; set; } = [];
        
        public virtual ICollection<Post> Posts { get; set; } = [];

        public string? InvestigationNotes { get; set; }

        public virtual ICollection<ApplicationUser> Reviewers { get; set; } = new List<ApplicationUser>();
    }
}
