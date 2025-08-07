using FraudFence.EntityModels.Dto.Newsletter;
using FraudFence.Web.Areas.Publisher.Models.Newsletters;
using FraudFence.Web.Infrastructure.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace FraudFence.Web.Areas.Publisher.Controllers
{
    [Area("Publisher")]
    [Authorize(Roles = "Publisher")]
    public class NewslettersController : Controller
    {
        private readonly NewsletterApiClient _newsletterApiClient;
        private readonly ArticleApiClient _articleApiClient;

        public NewslettersController(NewsletterApiClient newsletterApiClient, ArticleApiClient articleApiClient)
        {
            _newsletterApiClient = newsletterApiClient;
            _articleApiClient = articleApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var newsletters = await _newsletterApiClient.GetAllAsync();
            var vm = newsletters.Select(n => new NewsletterIndexViewModel
            {
                Id = n.Id,
                Subject = n.Subject,
                ScheduledAt = n.ScheduledAt,
                SentAt = n.SentAt,
                IsDisabled = n.IsDisabled
            }).ToList();

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var now = DateTime.Now;
            var articles = await _articleApiClient.GetAllAsync();

            var vm = new NewsletterCreateViewModel
            {
                ScheduledAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0),
                Articles = [.. articles.OrderBy(a => a.Title)
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
                var articles = await _articleApiClient.GetAllAsync();
                vm.Articles = articles.OrderBy(a => a.Title)
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Title
                    }).ToList();

                return View(vm);
            }

            var dto = new CreateNewsletterDTO(vm.Subject, vm.IntroText, vm.ScheduledAt, vm.SelectedArticleIds);
            await _newsletterApiClient.CreateAsync(dto);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var newsletter = await _newsletterApiClient.GetAsync(id);
            if (newsletter == null) return NotFound();

            var jsonElement = (JsonElement)newsletter;

            var vm = new NewsletterDetailsViewModel
            {
                Id = jsonElement.GetProperty("id").GetInt32(),
                Subject = jsonElement.GetProperty("subject").GetString()!,
                IntroText = jsonElement.TryGetProperty("introText", out var introTextProp) ? introTextProp.GetString() : null,
                ScheduledAt = jsonElement.GetProperty("scheduledAt").GetDateTime(),
                SentAt = jsonElement.TryGetProperty("sentAt", out var sentAtProp) && sentAtProp.ValueKind != JsonValueKind.Null ?
                    sentAtProp.GetDateTime() : null,
                Articles = [.. jsonElement.GetProperty("articles").EnumerateArray()
                    .Select(a => new NewsletterArticleViewModel
                    {
                        Id = a.GetProperty("id").GetInt32(),
                        Title = a.GetProperty("title").GetString()!,
                        CategoryName = a.GetProperty("categoryName").GetString()!,
                        IsDisabled = a.GetProperty("isDisabled").GetBoolean()
                    })]
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var newsletter = await _newsletterApiClient.GetAsync(id);
            if (newsletter == null) return NotFound();

            var articles = await _articleApiClient.GetAllAsync();
            var jsonElement = (JsonElement)newsletter;

            var vm = new NewsletterEditViewModel
            {
                Id = jsonElement.GetProperty("id").GetInt32(),
                Subject = jsonElement.GetProperty("subject").GetString()!,
                IntroText = jsonElement.TryGetProperty("introText", out var introTextProp) ? introTextProp.GetString() : null,
                ScheduledAt = jsonElement.GetProperty("scheduledAt").GetDateTime(),
                Articles = articles.OrderBy(a => a.Title)
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Title
                    }).ToList(),
                SelectedArticleIds = jsonElement.GetProperty("articles").EnumerateArray()
                    .Select(a => a.GetProperty("id").GetInt32()).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(NewsletterEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var articles = await _articleApiClient.GetAllAsync();
                vm.Articles = articles.OrderBy(a => a.Title)
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Title
                    }).ToList();
                return View(vm);
            }

            var dto = new UpdateNewsletterDTO(vm.Subject, vm.IntroText, vm.ScheduledAt, vm.SelectedArticleIds);
            await _newsletterApiClient.UpdateAsync(vm.Id, dto);

            TempData["notice"] = "Newsletter updated.";
            TempData["noticeBg"] = "alert-success";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _newsletterApiClient.DeleteAsync(id);
                TempData["notice"] = "Scheduled newsletter deleted.";
                TempData["noticeBg"] = "alert-success";
            }
            catch (HttpRequestException ex)
            {
                TempData["notice"] = ex.Message;
                TempData["noticeBg"] = "alert-danger";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
