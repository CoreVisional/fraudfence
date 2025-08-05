using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FraudFence.Web.Areas.Publisher.Models.Newsletters
{
    public class NewsletterEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Subject { get; set; } = null!;

        public string? IntroText { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ScheduledAt { get; set; }

        public IEnumerable<SelectListItem> Articles { get; set; } = [];

        [Required, MinLength(1, ErrorMessage = "Select at least one article")]
        public List<int> SelectedArticleIds { get; set; } = [];
    }
}
