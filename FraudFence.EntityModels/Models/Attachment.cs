using FraudFence.EntityModels.Common;

namespace FraudFence.EntityModels.Models
{
    public class Attachment : BaseEntity<int>
    {
        public required string FileName { get; set; }

        public required string ContentType { get; set; }

        public required string BucketName { get; set; }

        public required string Link { get; set; }
    }
}
