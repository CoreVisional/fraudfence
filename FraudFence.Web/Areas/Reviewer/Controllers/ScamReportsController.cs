using FraudFence.Data;
using FraudFence.EntityModels;
using FraudFence.EntityModels.Enums;
using FraudFence.EntityModels.Models;
using FraudFence.Web.Areas.Reviewer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using FraudFence.Interface.Common;
using FraudFence.Service;

namespace FraudFence.Web.Areas.Reviewer.Controllers
{
    [Area("Reviewer")]
    [Authorize(Roles = "Reviewer")]
    public class ScamReportsController : Controller
    {
        private readonly IReviewerService _reviewerService;
        private readonly ApplicationDbContext _context;
        private readonly ScamReportAttachmentService _scamReportAttachmentService;
        private readonly UserService _userService;

        public ScamReportsController(ScamReportAttachmentService scamReportAttachmentService,IReviewerService reviewerService, ApplicationDbContext context, UserService userService)
        {
            _reviewerService = reviewerService;
            _context = context;
            _scamReportAttachmentService = scamReportAttachmentService;
            _userService = userService;
        }

        public async Task<IActionResult> Index(string status)
        {
            var reports = await _reviewerService.GetScamReportsAsync(status);
            var viewModels = reports.Select(r => new ScamReportListViewModel
            {
                Id = r.Id,
                Title = r.Description.Length > 80 ? r.Description.Substring(0, 80) + "..." : r.Description,
                SubmittedBy = r.ReporterName,
                DateSubmitted = r.CreatedAt,
                Status = (ReportStatus)r.Status,
                Reviewer = r.Reviewers.Any() ? string.Join(", ", r.Reviewers.Select(u => u.Name)) : string.Empty,
                ExternalAgencyName = r.ExternalAgency != null ? r.ExternalAgency.Name : null
            }).ToList();
            ViewBag.SelectedStatus = status;
            return View(viewModels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var report = await _reviewerService.GetScamReportDetailsAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            var agencies = await _context.ExternalAgencies
                .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                }).ToListAsync();
            
            var allUsers = await _userService.GetUsersAsync();
            var allReviewers = allUsers
                .Where(u => u.Roles.Contains("Reviewer"))
                .Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Name
                }).ToList();

            List<ScamReportAttachment> scamReportAttachments = await
                _scamReportAttachmentService.GetScamReportAttachmentByScamReportId(id);
            
            List<string> imageLinks = new();
            
            foreach (ScamReportAttachment sra in scamReportAttachments)
            {
                imageLinks.Add(sra.Attachment.Link);
            }
            
            var viewModel = new ScamReportDetailsViewModel
            {
                Id = report.Id,
                Title = report.Description,
                Content = report.Description,
                SubmittedBy = report.ReporterName,
                DateSubmitted = report.CreatedAt,
                Status = (ReportStatus)report.Status,
                Reviewer = report.Reviewers.Any() ? string.Join(", ", report.Reviewers.Select(u => u.Name)) : "Unassigned",
                InvestigationNotes = report.InvestigationNotes ?? string.Empty,
                ExternalAgencyId = report.ExternalAgencyId,
                ExternalAgencyName = report.ExternalAgency?.Name,
                Agencies = agencies,
                SelectedReviewerIds = report.Reviewers.Select(u => u.Id).ToList(),
                AllReviewers = allReviewers,
                FirstEncounteredOn = report.FirstEncounteredOn,
                ScamReportAttachmentLinks = imageLinks
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ScamReportDetailsViewModel model)
        {
            var report = new ScamReport {
                Id = model.Id,
                Description = string.Empty,
                ReporterName = string.Empty,
                ReporterEmail = string.Empty,
                DynamicData = "{}"
            };
            await _reviewerService.UpdateScamReportAsync(
                report,
                model.SelectedReviewerIds,
                model.ExternalAgencyId,
                model.InvestigationNotes,
                model.Status
            );
            return RedirectToAction("Index");
        }
    }
}
