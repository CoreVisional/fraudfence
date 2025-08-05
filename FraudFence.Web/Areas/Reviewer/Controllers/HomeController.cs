using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FraudFence.Web.Areas.Reviewer.Controllers
{
    [Area("Reviewer")]
    [Authorize(Roles = "Reviewer")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
} 