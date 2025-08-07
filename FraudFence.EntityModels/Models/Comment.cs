using FraudFence.EntityModels.Common;

namespace FraudFence.EntityModels.Models
{
    public class Comment : BaseEntity<int>
    {
        public string UserId { get; set; }

        public int PostId { get; set; }

        public int? ParentCommentId { get; set; }

        public required string Content { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;

        public virtual Post Post { get; set; } = null!;

        public virtual Comment? ParentComment { get; set; } 
    }
}
