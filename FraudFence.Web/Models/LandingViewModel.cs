namespace FraudFence.Web.Models
{
    public class LandingViewModel
    {
        public List<ArticleCardViewModel> RecentArticles { get; set; } = [];

        public int ConsumerCount { get; set; }

        public int ReportSubmittedCount { get; set; }
    }
}
