using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FraudFence.Web.Areas.Consumer.Controllers
{
    [Area("Consumer")]
    [Authorize(Roles = "Consumer")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("MyFeed", "Post", new { area = "Consumer" });
        }
    }
}
