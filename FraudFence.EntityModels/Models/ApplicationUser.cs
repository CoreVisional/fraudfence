using FraudFence.EntityModels.Common;

namespace FraudFence.EntityModels.Models
{
    public class ApplicationUser : BaseEntity<string>
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? PhoneNumber { get; set; }

        public virtual ICollection<Setting> Settings { get; set; } = [];
        public virtual ICollection<ScamReport> SubmittedScamReports { get; set; } = [];
        public virtual ICollection<ScamReport> ReviewedScamReports { get; set; } = new List<ScamReport>();
        public virtual ICollection<Article> Articles { get; set; } = [];
        public virtual ICollection<Newsletter> Newsletters { get; set; } = [];
    }
}
