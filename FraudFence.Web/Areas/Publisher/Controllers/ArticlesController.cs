

using FraudFence.EntityModels.Dto.Article;
using FraudFence.Service;
using FraudFence.Web.Areas.Publisher.Models.Articles;
using FraudFence.Web.Infrastructure.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace FraudFence.Web.Areas.Publisher.Controllers
{
    [Area("Publisher")]
    [Authorize(Roles = "Publisher")]
    public class ArticlesController : Controller
    {
        private readonly ArticleApiClient _articleApiClient;
        private readonly ScamCategoryService _scamCategoryService;

        public ArticlesController(ArticleApiClient articleApiClient, ScamCategoryService scamCategoryService)
        {
            _articleApiClient = articleApiClient;
            _scamCategoryService = scamCategoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _articleApiClient.GetAllAsync();
            var vm = list.Select(a => new ArticleIndexViewModel
            {
                Id = a.Id,
                Title = a.Title,
                CategoryName = a.CategoryName,
                CanDelete = true
            }).ToList();

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
            if (!ModelState.IsValid) return View(FillCategories(vm));

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var dto = new CreateArticleDTO(vm.Title, vm.Content, vm.ScamCategoryId, userId);
            await _articleApiClient.CreateAsync(dto);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _articleApiClient.GetAsync(id);

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
                vm.Categories = [.. _scamCategoryService.GetAll().OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })];
                return View(vm);
            }

            var dto = new CreateArticleDTO(vm.Title, vm.Content, vm.ScamCategoryId, 0);
            await _articleApiClient.UpdateAsync(vm.Id, dto);

            TempData["notice"] = "Article updated successfully.";
            TempData["noticeBg"] = "alert-success";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var article = await _articleApiClient.GetAsync(id);

            if (article == null) return NotFound();

            var vm = new ArticleDetailsViewModel(article);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _articleApiClient.DeleteAsync(id);
            TempData["notice"] = "Article deleted.";
            TempData["noticeBg"] = "alert-success";

            return RedirectToAction(nameof(Index));
        }

        private ArticleCreateViewModel FillCategories(ArticleCreateViewModel vm)
        {
            vm.Categories = _scamCategoryService.GetAll().OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
            return vm;
        }
    }
}
