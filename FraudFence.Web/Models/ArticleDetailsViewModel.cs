using FraudFence.EntityModels.Models;
using System.Web;

namespace FraudFence.Web.Models
{
    public class ArticleDetailsViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public string CategoryName { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public ArticleDetailsViewModel(Article article)
        {
            Id = article.Id;
            Title = article.Title;
            Content = HttpUtility.HtmlDecode(article.Content);
            CategoryName = article.ScamCategory.Name;
            CreatedAt = article.CreatedAt;
        }
    }
}
