using FraudFence.EntityModels.common;

namespace FraudFence.EntityModels.Models
{
    public class ScamReportAttachment : BaseEntity
    {
        public int ScamReportId { get; set; }

        public int AttachmentId { get; set; }

        public ScamReport ScamReport { get; set; } = null!;

        public Attachment Attachment { get; set; } = null!;
    }
}
