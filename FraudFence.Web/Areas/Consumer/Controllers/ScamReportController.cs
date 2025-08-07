using FraudFence.Data;
using FraudFence.EntityModels.Enums;
using FraudFence.EntityModels.Models;
using FraudFence.Web.Areas.Consumer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using System.Text.Json;
using FraudFence.Service;
using FraudFence.Service.Common;
using System.Security.Claims;

namespace FraudFence.Web.Areas.Consumer.Controllers;

[Area("Consumer")]
public class ScamReportController(
    ScamReportService _scamReportService,
    ScamCategoryService _scamCategoryService,
    ExternalAgencyService _externalAgencyService,
    PostService _postService) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Post(int id)
    {
        int scamReportId = id;
        
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        ScamReport? scamReport = await _scamReportService.GetScamReport(scamReportId);

        if (scamReport == null)
        {
            return NotFound();
        }

        if (scamReport.Status != ReportStatus.Verified)
        {
            return Unauthorized("Unverified scam report");
        }

        if (scamReport.UserId != currentUserId)
        {
            return Unauthorized();
        }

        List<Post> checkPost = await _postService.GetPostsByScamReportId(scamReportId);

        if (checkPost.Count > 0)
        {
            return Conflict("Scam Report already exists");
        }

        Post post = new Post
        {
            UserId = currentUserId,
            Content = scamReport.Description,
            ScamReportId = scamReportId,
            CreatedAt = DateTime.Now,
            ModifiedBy = currentUserId,
            CreatedBy = currentUserId,
            LastModified = DateTime.Now
        };
        
        _postService.AddPost(post);
        
        TempData["SuccessMessage"] = "Post request sent successfully.";

        return RedirectToAction(nameof(ViewAll));
    }
    
    public async Task<IActionResult> ViewAll()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        var reports = await _scamReportService.GetScamReportsWithUserId(currentUserId);

        var viewModel = reports.Select(r => new ViewAllReportViewModel
        {
            Id = r.Id,
            ScamCategoryName = r.ScamCategory.Name,
            Description = r.Description,
            ReporterEmail = r.ReporterEmail,
            ReporterName = r.ReporterName,
            FirstEncounteredOn = r.FirstEncounteredOn,
            Status = r.Status,
            IsPosted = false
        }).ToList();
        
        var reportIds = viewModel.Select(r => r.Id).ToList();
        
        var relatedPosts = await _postService.GetScamReportIdWithStatus(reportIds);

        var postsByReportId = relatedPosts
            .GroupBy(p => p.ScamReportId)
            .ToDictionary(g => g.Key, g => g.Select(p => p.Status).ToList());

        foreach (var r in viewModel)
        {
            if (postsByReportId.TryGetValue(r.Id, out var postStatuses))
            {
                r.IsPosted = true;

                int statusValue = postStatuses.First();
                r.PostStatus = (FraudFence.EntityModels.Enums.PostStatus)statusValue;
            }
            else
            {
                r.IsPosted = false;
                r.PostStatus = null;
            }
        }

        return View(viewModel);
    }

    
    [HttpGet]
    public async Task<IActionResult> Submit()
    {
        List<ScamCategory> scamCategories = await _scamCategoryService.GetAllAsync();
        List<ExternalAgency> externalAgencies = await _externalAgencyService.GetAllAsync();

        SubmitReportViewModel submitReportViewModel = new SubmitReportViewModel();
        
        
        submitReportViewModel.FirstEncounteredOn = DateTime.Today;
        submitReportViewModel.ScamCategories = scamCategories.Select(category => new SelectListItem() { Text = category.Name, Value = category.Id.ToString() }).ToList();
        submitReportViewModel.ExternalAgencies = externalAgencies.Select(externalAgency => new SelectListItem() { Text = externalAgency.Name, Value = externalAgency.Id.ToString() }).ToList();
        
        return View(submitReportViewModel);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitReportViewModel submitReportViewModel)
    {
        if (!ModelState.IsValid)
        {
            List<ScamCategory> scamCategories = await _scamCategoryService.GetAllAsync();
            List<ExternalAgency> externalAgencies = await _externalAgencyService.GetAllAsync();
            
            submitReportViewModel.ScamCategories = scamCategories.Select(category => new SelectListItem() { Text = category.Name, Value = category.Id.ToString() }).ToList();
            submitReportViewModel.ExternalAgencies = externalAgencies.Select(externalAgency => new SelectListItem() { Text = externalAgency.Name, Value = externalAgency.Id.ToString() }).ToList();
            
            return View(submitReportViewModel);
        }
        
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserName = User.Identity?.Name;
        var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

        if (User.Identity?.IsAuthenticated == true)
        {
            submitReportViewModel.ReporterName = currentUserName;
            submitReportViewModel.ReporterEmail = currentUserEmail;
        }
        
        ScamReport report = new ScamReport
        {
            ScamCategoryId = submitReportViewModel.ScamCategoryId,
            ExternalAgencyId = submitReportViewModel.ExternalAgencyId,
            FirstEncounteredOn = submitReportViewModel.FirstEncounteredOn,
            Description = submitReportViewModel.Description,
            ReporterName = submitReportViewModel.ReporterName,
            ReporterEmail = submitReportViewModel.ReporterEmail,
            UserId = currentUserId,
            CreatedAt = DateTime.Now,
            LastModified = DateTime.Now,
            CreatedBy = currentUserId,
            ModifiedBy = currentUserId,
            DynamicData = "{}",
            Status = ReportStatus.Submitted
        };
        
        _scamReportService.AddScamReport(report);
        
        
        TempData["Success"] = "Scam report submitted successfully!";
        return RedirectToAction("Submit");
    }
}