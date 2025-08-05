using FraudFence.EntityModels.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Web;

namespace FraudFence.Web.Areas.Publisher.Models.Articles
{
    public class ArticleEditViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public int ScamCategoryId { get; set; }

        public string Content { get; set; } = null!;

        public IEnumerable<SelectListItem> Categories { get; set; } = [];

        public ArticleEditViewModel() { }

        public ArticleEditViewModel(Article article)
        {
            Id = article.Id;
            Title = article.Title;
            ScamCategoryId = article.ScamCategoryId;
            Content = HttpUtility.HtmlDecode(article.Content);
        }
    }
}
