using FraudFence.Service;
using FraudFence.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FraudFence.Web.Areas.Consumer.Controllers
{
    [Area("Consumer")]
    [Authorize(Roles = "Consumer")]
    public class ArticlesController : Controller
    {
        private readonly ArticleService _articleService;

        public ArticlesController(ArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var articles = await _articleService.GetAll()
                                .Include(a => a.ScamCategory)
                                .OrderByDescending(a => a.CreatedAt)
                                .ToListAsync();

            var cards = articles.Select(a => new ArticleCardViewModel(a)).ToList();

            return View(cards);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var article = await _articleService.GetAll()
                           .Include(a => a.ScamCategory)
                           .FirstOrDefaultAsync(a => a.Id == id);
            if (article is null) return NotFound();

            return View(new ArticleDetailsViewModel(article));
        }
    }
}
