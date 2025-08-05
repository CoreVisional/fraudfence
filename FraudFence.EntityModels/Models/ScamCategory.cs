using FraudFence.EntityModels.common;

namespace FraudFence.EntityModels.Models
{
    public class ScamCategory : BaseEntity
    {
        public required string Name { get; set; }

        public int? ParentCategoryId { get; set; }

        public virtual ScamCategory ParentCategory { get; set; } = null!;

        public virtual ICollection<ScamCategory> Subcategories { get; set; } = [];

        public virtual ICollection<ScamReport> Reports { get; set; } = [];

        public virtual ICollection<Article> Articles { get; set; } = [];

        public virtual ICollection<Setting> UserPreferences { get; set; } = [];
    }
}
