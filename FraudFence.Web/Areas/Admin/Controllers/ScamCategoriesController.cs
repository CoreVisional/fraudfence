using FraudFence.Data;
using FraudFence.EntityModels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using FraudFence.Service;

namespace FraudFence.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ScamCategoriesController : Controller
    {
        private readonly ScamCategoryService _service;
        public ScamCategoriesController(ScamCategoryService service)
        {
            _service = service;
        }

        // GET: Admin/ScamCategories
        public async Task<IActionResult> Index(string search = null, bool? showDisabled = null)
        {
            var categories = await _service.GetAllWithParentAsync(search, showDisabled);
            ViewBag.CurrentSearch = search;
            ViewBag.ShowDisabled = showDisabled;
            return View(categories);
        }

        // GET: Admin/ScamCategories/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _service.GetAllAsync();
            return View();
        }

        // POST: Admin/ScamCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateScamCategoryViewModel model)
        {
            ViewBag.Categories = await _service.GetAllAsync();
            if (!ModelState.IsValid)
                return View(model);
            var category = new ScamCategory
            {
                Name = model.Name,
                ParentCategoryId = model.ParentCategoryId
            };
            await _service.AddAsync(category);
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/ScamCategories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null) return NotFound();
            ViewBag.Categories = (await _service.GetAllAsync()).Where(c => c.Id != id).ToList();
            var model = new EditScamCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ParentCategoryId = category.ParentCategoryId
            };
            return View(model);
        }

        // POST: Admin/ScamCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditScamCategoryViewModel model)
        {
            ViewBag.Categories = (await _service.GetAllAsync()).Where(c => c.Id != model.Id).ToList();
            if (!ModelState.IsValid)
                return View(model);
            var category = await _service.GetByIdAsync(model.Id);
            if (category == null) return NotFound();
            category.Name = model.Name;
            category.ParentCategoryId = model.ParentCategoryId;
            await _service.UpdateAsync(category);
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/ScamCategories/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null) return NotFound();
            if (category.IsDisabled)
                await _service.EnableAsync(category);
            else
                await _service.DisableAsync(category);
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/ScamCategories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null) return NotFound();
            await _service.DeleteAsync(category);
            return RedirectToAction(nameof(Index));
        }

        public class CreateScamCategoryViewModel
        {
            public string Name { get; set; }
            public int? ParentCategoryId { get; set; }
        }
        public class EditScamCategoryViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? ParentCategoryId { get; set; }
        }
    }
} 