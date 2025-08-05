using FraudFence.Service;
using FraudFence.Web.Areas.Publisher.Models;
using FraudFence.Web.Areas.Publisher.Models.Newsletters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FraudFence.Web.Areas.Publisher.Controllers
{
    [Area("Publisher")]
    [Authorize(Roles = "Publisher")]
    public class HomeController : Controller
    {
        private readonly ArticleService _articleService;
        private readonly NewsletterService _newsletterService;

        public HomeController(ArticleService articleService, NewsletterService newsletterService)
        {
            _articleService = articleService;
            _newsletterService = newsletterService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var allArticles = _articleService.GetAll();
            var totalArticles = allArticles.Count();

            var scheduledArticleIds = _newsletterService.GetAll()
                .SelectMany(n => n.Articles.Select(a => a.Id))
                .Distinct()
                .ToHashSet();
            var draftArticles = allArticles.Count(a => !scheduledArticleIds.Contains(a.Id));

            var pendingNewsletters = _newsletterService.GetAll()
                .Count(n => n.SentAt == null);

            var now = DateTime.Now;
            var sentThisMonth = _newsletterService.GetAll()
                .Count(n => n.SentAt.HasValue
                            && n.SentAt.Value.Year == now.Year
                            && n.SentAt.Value.Month == now.Month);

            var upcoming = _newsletterService
                .GetAll()
                .Where(n => n.SentAt == null)
                .OrderBy(n => n.ScheduledAt)
                .Select(n => new NewsletterIndexViewModel
                {
                    Id = n.Id,
                    Subject = n.Subject,
                    ScheduledAt = n.ScheduledAt,
                    SentAt = n.SentAt
                })
                .ToList();

            var vm = new PublisherDashboardViewModel
            {
                TotalArticles = totalArticles,
                DraftArticles = draftArticles,
                PendingNewsletters = pendingNewsletters,
                SentThisMonth = sentThisMonth,
                UpcomingNewsletters = upcoming
            };

            return View(vm);
        }
    }
}
