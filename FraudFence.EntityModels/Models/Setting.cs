using FraudFence.EntityModels.Common;

namespace FraudFence.EntityModels.Models
{
    public class Setting : BaseEntity<int>
    {
        public string UserId { get; set; }

        public int ScamCategoryId { get; set; }

        public bool Subscribed { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ScamCategory ScamCategory { get; set; } = null!;
    }
}
