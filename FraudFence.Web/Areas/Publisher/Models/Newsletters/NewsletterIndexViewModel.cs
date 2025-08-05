namespace FraudFence.Web.Areas.Publisher.Models.Newsletters
{
    public class NewsletterIndexViewModel
    {
        public int Id { get; set; }

        public string Subject { get; set; } = null!;

        public DateTime ScheduledAt { get; set; }

        public DateTime? SentAt { get; set; }

        public bool IsDisabled { get; set; }

        public string Status
        {
            get
            {
                if (!SentAt.HasValue && !IsDisabled) return "Pending";
                if (SentAt.HasValue && IsDisabled) return "Sent";
                if (!SentAt.HasValue && IsDisabled) return "Cancelled";
                // Check what's going on if this is ever hit
                return "Unknown";
            }
        }
    }
}
