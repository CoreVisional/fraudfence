using FraudFence.EntityModels.Dto;
using FraudFence.EntityModels.Models;
using FraudFence.Service;
using FraudFence.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FraudFence.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly UserService _userService;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountsController(UserService userService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegistrationViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = new RegistrationDTO(vm.Name, vm.Email, vm.Password);

            var (result, user) = await _userService.CreateAsync(dto);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(vm);
            }

            await _signInManager.SignInAsync(user!, isPersistent: false);

            return RedirectToAction("Index", "Home", new { area = "Consumer" });
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");

                return View(vm);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, vm.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.IsLockedOut
                    ? "Account locked. Try again later."
                    : "Invalid email or password.");
                return View(vm);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            var roles = await _userManager.GetRolesAsync(user);
            var area = roles.Contains("Admin") ? "Admin"
                       : roles.Contains("Publisher") ? "Publisher"
                       : roles.Contains("Reviewer") ? "Reviewer"
                       : "Consumer";

            return RedirectToAction("Index", "Home", new { area });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
