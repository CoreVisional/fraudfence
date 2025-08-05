using FraudFence.EntityModels.common;

namespace FraudFence.EntityModels.Models
{
    public class PostAttachment : BaseEntity
    {
        public int PostId { get; set; }

        public int AttachmentId { get; set; }

        public Post Post { get; set; } = null!;

        public Attachment Attachment { get; set; } = null!;
    }
}
