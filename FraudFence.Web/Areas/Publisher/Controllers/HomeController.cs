using FraudFence.Web.Areas.Publisher.Models;
using FraudFence.Web.Areas.Publisher.Models.Newsletters;
using FraudFence.Web.Infrastructure.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FraudFence.Web.Areas.Publisher.Controllers
{
    [Area("Publisher")]
    [Authorize(Roles = "Publisher")]
    public class HomeController : Controller
    {
        private readonly ArticleApiClient _articleApiClient;
        private readonly NewsletterApiClient _newsletterApiClient;

        public HomeController(ArticleApiClient articleApiClient, NewsletterApiClient newsletterApiClient)
        {
            _articleApiClient = articleApiClient;
            _newsletterApiClient = newsletterApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allArticles = await _articleApiClient.GetAllAsync();
            var totalArticles = allArticles.Count;

            var allNewsletters = await _newsletterApiClient.GetAllAsync();

            var fullNewsletters = await Task.WhenAll(
                allNewsletters.Select(n => _newsletterApiClient.GetWithArticlesAsync(n.Id)));

            var scheduledArticleIds = fullNewsletters
                .Where(n => n != null)
                .SelectMany(n => n!.Articles.Select(a => a.Id))
                .ToHashSet();

            var draftArticles = allArticles.Count(a => !scheduledArticleIds.Contains(a.Id));
            var pendingNewsletters = allNewsletters.Count(n => n.SentAt == null);

            var now = DateTime.Now;
            var sentThisMonth = allNewsletters.Count(n =>
                                  n.SentAt is { } sent &&
                                  sent.Year == now.Year &&
                                  sent.Month == now.Month);

            var upcoming = allNewsletters
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
