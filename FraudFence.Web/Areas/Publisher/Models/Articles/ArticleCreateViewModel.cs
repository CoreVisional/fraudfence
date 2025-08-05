using Microsoft.AspNetCore.Mvc.Rendering;

namespace FraudFence.Web.Areas.Publisher.Models.Articles
{
    public class ArticleCreateViewModel
    {
        public string Title { get; set; } = null!;

        public int ScamCategoryId { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; } = [];

        public string Content { get; set; } = null!;
    }
}
