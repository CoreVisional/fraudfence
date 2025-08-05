namespace FraudFence.Web.Areas.Publisher.Models.Articles
{
    public class ArticleIndexViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string CategoryName { get; set; } = null!;

        public bool CanDelete { get; set; }
    }
}
