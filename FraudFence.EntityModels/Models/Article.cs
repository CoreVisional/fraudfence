using FraudFence.EntityModels.Common;

namespace FraudFence.EntityModels.Models
{
    public class Article : BaseEntity<int>
    {
        public string UserId { get; set; }

        public int ScamCategoryId { get; set; }

        public required string Title { get; set; }

        public required string Content { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ScamCategory ScamCategory { get; set; } = null!;

        public virtual ICollection<Newsletter> Newsletters { get; set; } = [];
    }
}
