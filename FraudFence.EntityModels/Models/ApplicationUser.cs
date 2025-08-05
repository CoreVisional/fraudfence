using Microsoft.AspNetCore.Identity;

namespace FraudFence.EntityModels.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public required string Name { get; set; }

        public virtual ICollection<Setting> Settings { get; set; } = [];

        public virtual ICollection<ScamReport> SubmittedScamReports { get; set; } = [];

        public virtual ICollection<ScamReport> ReviewedScamReports { get; set; } = new List<ScamReport>();

        public virtual ICollection<Article> Articles { get; set; } = [];

        public virtual ICollection<Newsletter> Newsletters { get; set; } = [];
    }
}
