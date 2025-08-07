using FraudFence.EntityModels.Common;

namespace FraudFence.EntityModels.Models
{
    public class ExternalAgency : BaseEntity<int>
    {
        public required string Name { get; set; }

        public required string Email { get; set; }

        public required string Phone { get; set; }

        public virtual ICollection<ScamReport> ScamReports { get; set; } = [];
    }
}
