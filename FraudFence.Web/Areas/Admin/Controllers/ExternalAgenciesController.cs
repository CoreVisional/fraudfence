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
    public class ExternalAgenciesController : Controller
    {
        private readonly ExternalAgencyService _service;
        public ExternalAgenciesController(ExternalAgencyService service)
        {
            _service = service;
        }

        // GET: Admin/ExternalAgencies
        public async Task<IActionResult> Index(string search = null, bool? showDisabled = null)
        {
            var agencies = await _service.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(search))
            {
                agencies = agencies.Where(a => a.Name.Contains(search) || a.Email.Contains(search) || a.Phone.Contains(search)).ToList();
            }
            if (showDisabled.HasValue)
            {
                agencies = agencies.Where(a => a.IsDisabled == showDisabled.Value).ToList();
            }
            agencies = agencies.OrderBy(a => a.Name).ToList();
            ViewBag.CurrentSearch = search;
            ViewBag.ShowDisabled = showDisabled;
            return View(agencies);
        }

        // GET: Admin/ExternalAgencies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/ExternalAgencies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateExternalAgencyViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            // Check for duplicate email
            var existing = (await _service.GetAllAsync()).FirstOrDefault(a => a.Email == model.Email);
            if (existing != null)
            {
                ModelState.AddModelError("Email", "An agency with this email already exists.");
                return View(model);
            }
            var agency = new ExternalAgency
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone
            };
            await _service.AddAsync(agency);
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/ExternalAgencies/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var agency = await _service.GetByIdAsync(id);
            if (agency == null) return NotFound();
            var model = new EditExternalAgencyViewModel
            {
                Id = agency.Id,
                Name = agency.Name,
                Email = agency.Email,
                Phone = agency.Phone
            };
            return View(model);
        }

        // POST: Admin/ExternalAgencies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditExternalAgencyViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            // Check for duplicate email (ignore current agency)
            var existing = (await _service.GetAllAsync()).FirstOrDefault(a => a.Email == model.Email && a.Id != model.Id);
            if (existing != null)
            {
                ModelState.AddModelError("Email", "An agency with this email already exists.");
                return View(model);
            }
            var agency = await _service.GetByIdAsync(model.Id);
            if (agency == null) return NotFound();
            agency.Name = model.Name;
            agency.Email = model.Email;
            agency.Phone = model.Phone;
            await _service.UpdateAsync(agency);
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/ExternalAgencies/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var agency = await _service.GetByIdAsync(id);
            if (agency == null) return NotFound();
            if (agency.IsDisabled)
                await _service.EnableAsync(agency);
            else
                await _service.DisableAsync(agency);
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/ExternalAgencies/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var agency = await _service.GetByIdAsync(id);
            if (agency == null) return NotFound();
            await _service.DeleteAsync(agency);
            return RedirectToAction(nameof(Index));
        }

        public class CreateExternalAgencyViewModel
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
        }
        public class EditExternalAgencyViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
        }
    }
} 