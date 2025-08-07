using FraudFence.EntityModels.Common;

namespace FraudFence.EntityModels.Models
{
    public class Post : BaseEntity<int>
    {
        public string UserId { get; set; }

        public required string Content { get; set; }
        
        public int ScamReportId { get; set; }

        public int Status { get; set; } = (int)FraudFence.EntityModels.Enums.PostStatus.Pending;

        public virtual ScamReport ScamReport { get; set; } = null!;

        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ICollection<PostAttachment> PostAttachments { get; set; } = [];

        public virtual ICollection<Comment> Comments { get; set; } = [];
    }
}
