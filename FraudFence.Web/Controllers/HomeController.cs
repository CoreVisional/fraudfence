using FraudFence.Service;
using FraudFence.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FraudFence.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ArticleService _articleService;
        private readonly UserService _userService;
        private readonly ScamReportService _scamReportService;

        public HomeController(ArticleService articleService, UserService userService, ScamReportService scamReportService)
        {
            _articleService = articleService;
            _userService = userService;
            _scamReportService = scamReportService;
        }

        public async Task<IActionResult> Index()
        {
            var recent = await _articleService.GetAll(getDisabled: false)
                .Include(a => a.ScamCategory)
                .OrderByDescending(a => a.CreatedAt)
                .Take(3)
                .ToListAsync();

            var cards = recent.Select(a => new ArticleCardViewModel(a)).ToList();

            var users = await _userService.GetUsersAsync();

            var consumerCount = users
                .Where(u => u.Roles.Contains("Consumer") && u.IsActive)
                .Count();

            var reportCount = await _scamReportService.GetAll().CountAsync();

            var vm = new LandingViewModel
            {
                RecentArticles = cards,
                ConsumerCount = consumerCount,
                ReportSubmittedCount = reportCount
            };

            return View(vm);
        }

        [HttpGet]
        [Route("/Home/Error/{statusCode}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode)
        {
            if (statusCode < 400) return RedirectToAction("Index");

            return View(
                new ErrorViewModel { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    StatusCode = statusCode 
                });
        }
    }
}
