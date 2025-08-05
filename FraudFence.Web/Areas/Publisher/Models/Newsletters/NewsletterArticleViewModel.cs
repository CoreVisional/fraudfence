namespace FraudFence.Web.Areas.Publisher.Models.Newsletters
{
    public class NewsletterArticleViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string CategoryName { get; set; } = null!;

        public bool IsDisabled { get; set; }
    }
}
