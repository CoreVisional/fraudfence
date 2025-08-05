using FraudFence.Web.Areas.Publisher.Models.Newsletters;

namespace FraudFence.Web.Areas.Publisher.Models
{
    public class PublisherDashboardViewModel
    {
        public int TotalArticles { get; set; }

        public int DraftArticles { get; set; }

        public int PendingNewsletters { get; set; }

        public int SentThisMonth { get; set; }

        public IEnumerable<NewsletterIndexViewModel> UpcomingNewsletters { get; set; } = [];
    }
}
