using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FraudFence.EntityModels.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using FraudFence.EntityModels.Dto;
using FraudFence.Service;

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
        public async Task<IActionResult> Index(string search = null, string role = null)
        {
            var userViewModels = await _userService.GetUsersAsync(search, role);
            var allRoles = _userService.GetAllRoles();
            ViewBag.AllRoles = allRoles;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentRole = role;
            return View(userViewModels);
        }

        // GET: Admin/Users/Create
        public IActionResult Create()
        {
            var roles = _userService.GetAllRoles(excludeAdmin: true);
            ViewBag.Roles = roles;
            return View();
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            var roles = _userService.GetAllRoles(excludeAdmin: true);
            ViewBag.Roles = roles;
            if (!ModelState.IsValid)
                return View(model);
            var (result, user) = await _userService.CreateUserAsync(model);
            if (result.Succeeded)
                return RedirectToAction(nameof(Index));
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _userService.GetEditUserViewModelAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var result = await _userService.EditUserAsync(model);
            if (result.Succeeded)
                return RedirectToAction(nameof(Index));
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        // POST: Admin/Users/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            await _userService.ToggleActiveAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.DeleteUserAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
} 