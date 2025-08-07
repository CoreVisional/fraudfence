using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FraudFence.EntityModels.Dto;
using FraudFence.Service;
using System.Threading.Tasks;

namespace FraudFence.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(string search = null)
        {
            var userViewModels = await _userService.GetUsersAsync();
            // Filtering logic will be added back later if needed
            ViewBag.CurrentSearch = search;
            return View(userViewModels);
        }

        // GET: Admin/Users/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new List<string> { "Admin", "Publisher", "Reviewer", "Consumer" };
            return View();
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _userService.CreateUserAsync(model);
            if (success)
                return RedirectToAction(nameof(Index));
            
            ModelState.AddModelError(string.Empty, "An error occurred while creating the user.");
            return View(model);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var model = await _userService.GetUserByIdAsync(id);
            if (model == null) return NotFound();
            
            var editModel = new EditUserViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            return View(editModel);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _userService.EditUserAsync(model);
            if (success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError(string.Empty, "An error occurred while updating the user.");
            return View(model);
        }

        // POST: Admin/Users/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            await _userService.ToggleActiveAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            await _userService.DeleteUserAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}