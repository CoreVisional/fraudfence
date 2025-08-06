using FraudFence.EntityModels.Dto.Article;
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

        public ArticleDetailsViewModel(ArticleDTO dto)
        {
            Id = dto.Id;
            Title = dto.Title;
            CategoryName = dto.CategoryName;
            Content = HttpUtility.HtmlDecode(dto.Content);
            CreatedAt = dto.CreatedAt;
            UpdatedAt = dto.LastModified;
        }
    }
}
