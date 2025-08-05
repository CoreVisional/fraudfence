using FraudFence.EntityModels.Models;
using FraudFence.Service;
using FraudFence.Web.Areas.Publisher.Models.Newsletters;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FraudFence.Web.Areas.Publisher.Controllers
{
    [Area("Publisher")]
    [Authorize(Roles = "Publisher")]
    public class NewslettersController : Controller
    {
        private readonly NewsletterService _newsletterService;
        private readonly ArticleService _articleService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public NewslettersController(NewsletterService newsletterService, ArticleService articleService, IBackgroundJobClient backgroundJobClient)
        {
            _newsletterService = newsletterService;
            _articleService = articleService;
            _backgroundJobClient = backgroundJobClient;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var vm = _newsletterService.GetAll(getDisabled: true)
                .OrderBy(n => n.ScheduledAt)
                .Select(n => new NewsletterIndexViewModel
                {
                    Id = n.Id,
                    Subject = n.Subject,
                    ScheduledAt = n.ScheduledAt,
                    SentAt = n.SentAt,
                    IsDisabled = n.IsDisabled
                })
                .ToList();

            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var now = DateTime.Now;

            var vm = new NewsletterCreateViewModel
            {
                ScheduledAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0),
                Articles = [.. _articleService.GetAll()
                    .OrderBy(a => a.Title)
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Title
                    })]
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(NewsletterCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var now = DateTime.Now;

                vm.ScheduledAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

                vm.Articles = [.. _articleService.GetAll()
                    .OrderBy(a => a.Title)
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Title
                    })];

                return View(vm);
            }

            var newsletter = new Newsletter
            {
                Subject = vm.Subject,
                IntroText = vm.IntroText,
                ScheduledAt = vm.ScheduledAt
            };

            var articles = _articleService.GetAll()
                .Where(a => vm.SelectedArticleIds.Contains(a.Id))
                .ToList();

            foreach (var art in articles) newsletter.Articles.Add(art);

            await _newsletterService.AddAsync(newsletter);

            var jobId = _backgroundJobClient.Schedule(
                () => _newsletterService.SendNewsletterAsync(newsletter.Id),
                newsletter.ScheduledAt
            );

            newsletter.HangfireJobId = jobId;
            await _newsletterService.UpdateAsync(newsletter);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var newsletter = _newsletterService
                .GetAll(getDisabled: true)
                .Include(n => n.Articles)
                    .ThenInclude(a => a.ScamCategory)
                .FirstOrDefault(n => n.Id == id);

            if (newsletter == null)
                return NotFound();

            var vm = new NewsletterDetailsViewModel
            {
                Id = newsletter.Id,
                Subject = newsletter.Subject,
                IntroText = newsletter.IntroText,
                ScheduledAt = newsletter.ScheduledAt,
                SentAt = newsletter.SentAt,
                Articles = [.. newsletter.Articles
                    .Select(a => new NewsletterArticleViewModel
                    {
                        Id = a.Id,
                        Title = a.Title,
                        CategoryName = a.ScamCategory.Name,
                        IsDisabled = a.IsDisabled,
                    })]
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var nl = _newsletterService
                .GetAll(getDisabled: true)
                .Include(x => x.Articles)
                .FirstOrDefault(x => x.Id == id);

            if (nl == null) return NotFound();

            var all = _articleService.GetAll()
                .OrderBy(a => a.Title)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Title
                });

            var vm = new NewsletterEditViewModel
            {
                Id = nl.Id,
                Subject = nl.Subject,
                IntroText = nl.IntroText,
                ScheduledAt = nl.ScheduledAt,
                Articles = all,
                SelectedArticleIds = nl.Articles.Select(a => a.Id).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(NewsletterEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Articles = _articleService.GetAll()
                    .OrderBy(a => a.Title)
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Title
                    }).ToList();
                return View(vm);
            }

            var nl = _newsletterService
                .GetAll(getDisabled: true)
                .Include(x => x.Articles)
                .FirstOrDefault(x => x.Id == vm.Id);
            if (nl == null) return NotFound();

            nl.Subject = vm.Subject;
            nl.IntroText = vm.IntroText;
            nl.ScheduledAt = vm.ScheduledAt;

            nl.Articles.Clear();
            var toAdd = _articleService.GetAll()
                .Where(a => vm.SelectedArticleIds.Contains(a.Id));
            foreach (var art in toAdd) nl.Articles.Add(art);

            await _newsletterService.UpdateAsync(nl);

            var jobId = _backgroundJobClient.Schedule(
                () => _newsletterService.SendNewsletterAsync(nl.Id),
                nl.ScheduledAt
            );

            nl.HangfireJobId = jobId;
            await _newsletterService.UpdateAsync(nl);

            TempData["notice"] = "Newsletter updated.";
            TempData["noticeBg"] = "alert-success";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var nl = _newsletterService.GetById(id);
            if (nl == null) return NotFound();

            try
            {
                await _newsletterService.DeleteAsync(nl);
                TempData["notice"] = "Scheduled newsletter deleted.";
                TempData["noticeBg"] = "alert-success";
            }
            catch (InvalidOperationException ex)
            {
                TempData["notice"] = ex.Message;
                TempData["noticeBg"] = "alert-danger";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
