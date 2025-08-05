using FraudFence.EntityModels.Models;
using FraudFence.Service;
using FraudFence.Web.Areas.Publisher.Models.Articles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FraudFence.Web.Areas.Publisher.Controllers
{
    [Area("Publisher")]
    [Authorize(Roles = "Publisher")]
    public class ArticlesController : Controller
    {
        private readonly ArticleService _articleService;
        private readonly ScamCategoryService _scamCategoryService;
        private readonly NewsletterService _newsletterService;

        public ArticlesController(ArticleService articleService, ScamCategoryService scamCategoryService, NewsletterService newsletterService)
        {
            _articleService = articleService;
            _scamCategoryService = scamCategoryService;
            _newsletterService = newsletterService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var blocked = _newsletterService.GetAll(getDisabled: true)
                .Where(n => n.SentAt == null)
                .SelectMany(n => n.Articles.Select(a => a.Id))
                .Distinct()
                .ToHashSet();

            var vm = _articleService.GetAll()
                .Include(a => a.ScamCategory)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new ArticleIndexViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    CategoryName = a.ScamCategory.Name,
                    CanDelete = !blocked.Contains(a.Id)
                })
                .ToList();

            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var items = _scamCategoryService
                .GetAll()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            var vm = new ArticleCreateViewModel
            {
                Categories = items
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ArticleCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = [.. _scamCategoryService
                    .GetAll()
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })];

                return View(vm);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var article = new Article
            {
                Title = vm.Title,
                Content = vm.Content,
                ScamCategoryId = vm.ScamCategoryId,
                UserId = userId
            };

            await _articleService.AddAsync(article);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var article = _articleService.GetById(id);

            if (article == null)
            {
                return NotFound();
            }

            var categories = _scamCategoryService
                .GetAll()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            var vm = new ArticleEditViewModel(article)
            {
                Categories = categories
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ArticleEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = [.. _scamCategoryService
                    .GetAll()
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })];

                return View(vm);
            }

            var article = new Article
            {
                Id = vm.Id,
                ScamCategoryId = vm.ScamCategoryId,
                Title = vm.Title,
                Content = vm.Content,
                LastModified = DateTime.Now
            };

            await _articleService.UpdateAsync(article);

            TempData["notice"] = "Articles updated successfully.";
            TempData["noticeBg"] = "alert-success";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var article = _articleService.GetAll(getDisabled: true)
                .Include(a => a.ScamCategory)
                .FirstOrDefault(a => a.Id == id);

            if (article == null) return NotFound();

            var vm = new ArticleDetailsViewModel(article);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var article = _articleService.GetById(id);

            if (article == null) return NotFound();

            try
            {
                await _articleService.DeleteAsync(article);
                TempData["notice"] = "Article deleted.";
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
