namespace FraudFence.Web.Areas.Publisher.Models.Newsletters
{
    public class NewsletterDetailsViewModel
    {
        public int Id { get; set; }

        public string Subject { get; set; } = null!;

        public string? IntroText { get; set; }

        public DateTime ScheduledAt { get; set; }

        public DateTime? SentAt { get; set; }

        public List<NewsletterArticleViewModel> Articles { get; set; } = [];
    }
}
