using FraudFence.EntityModels.Models;
using System.Web;

namespace FraudFence.Web.Areas.Publisher.Models.Articles
{
    public class ArticleDetailsViewModel
    {
        public int Id { get; }

        public string Title { get; }

        public string CategoryName { get; }

        public string Content { get; }

        public DateTime CreatedAt { get; }

        public DateTime? UpdatedAt { get; }

        public ArticleDetailsViewModel(Article article)
        {
            Id = article.Id;
            Title = article.Title;
            CategoryName = article.ScamCategory.Name;
            Content = HttpUtility.HtmlDecode(article.Content);
            CreatedAt = article.CreatedAt;
            UpdatedAt = article.LastModified;
        }
    }
}
