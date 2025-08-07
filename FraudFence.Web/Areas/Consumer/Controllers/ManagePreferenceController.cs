using FraudFence.Data;
using FraudFence.EntityModels.Models;
using FraudFence.Service;
using FraudFence.Web.Areas.Consumer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FraudFence.Web.Areas.Consumer.Controllers;

[Area("Consumer")]
[Authorize]
public class ManagePreferenceController(SettingService settingService, ScamCategoryService scamCategoryService) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleSubscription(int scamCategoryId, bool subscribed)
    {
        var currentUserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        var setting = await settingService.GetSettingByUserIdAndScamCategoryId(currentUserId, scamCategoryId);

        if (setting == null)
        {
            setting = new Setting
            {
                UserId = currentUserId,
                ScamCategoryId = scamCategoryId,
                Subscribed = subscribed,
                CreatedAt = DateTime.Now,
                LastModified = DateTime.Now,
                CreatedBy = currentUserId,
                ModifiedBy = currentUserId
            };
            settingService.AddSetting(setting);
        }
        else
        {
            setting.Subscribed = subscribed;
            setting.LastModified = DateTime.Now;
            setting.ModifiedBy = currentUserId;
        }

        await settingService.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Index()
    {
        var currentUserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        var scamCategories = await scamCategoryService.GetAllAsync();

        var settings = await settingService.GetAllSettingsByUserIdAsync(currentUserId);

        var viewModels = scamCategories.Select(sc =>
        {
            var userSetting = settings.FirstOrDefault(s => s.ScamCategoryId == sc.Id && s.UserId == currentUserId);
    
            return new ManagePreferenceViewModel
            {
                ScamCategoryId = sc.Id,
                ScamCategoryName = sc.Name,
                Subscribed = userSetting?.Subscribed == true
            };
        }).ToList();

        return View(viewModels);
    }
}