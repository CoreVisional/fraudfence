using FraudFence.EntityModels.Models;
using System.Web;

namespace FraudFence.Web.Models
{
    public class ArticleCardViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string Excerpt { get; set; } = null!;

        public string CategoryName { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public ArticleCardViewModel(Article article)
        {
            Id = article.Id;
            Title = article.Title;

            var plain = HttpUtility.HtmlDecode(article.Content);

            Excerpt = plain.Length > 200 ? plain[..200] + "…" : plain;

            CategoryName = article.ScamCategory.Name;
            CreatedAt = article.CreatedAt;
        }
    }
}
