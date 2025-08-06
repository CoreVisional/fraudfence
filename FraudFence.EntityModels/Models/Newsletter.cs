using FraudFence.EntityModels.common;

namespace FraudFence.EntityModels.Models
{
    public class Newsletter : BaseEntity
    {
        public required string Subject { get; set; }

        public string? IntroText { get; set; }

        public DateTime ScheduledAt { get; set; }

        public DateTime? SentAt { get; set; }

        public virtual ICollection<Article> Articles { get; set; } = [];
    }
}
