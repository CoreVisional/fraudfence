using FraudFence.Data;
using FraudFence.EntityModels.Models;
using FraudFence.Service;
using FraudFence.Web.Areas.Consumer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FraudFence.Web.Areas.Consumer.Controllers;

[Area("Consumer")]
[Authorize(Roles = "Consumer")]
public class ManagePreferenceController(UserManager<ApplicationUser> userManager, SettingService settingService, ScamCategoryService scamCategoryService) : Controller
{
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleSubscription(int scamCategoryId, bool subscribed)
    {
        var currentUser = await userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Unauthorized();
        }

        var setting = await settingService.GetSettingByUserIdAndScamCategoryId(currentUser.Id, scamCategoryId);

        if (setting == null)
        {
            setting = new Setting
            {
                UserId = currentUser.Id,
                ScamCategoryId = scamCategoryId,
                Subscribed = subscribed,
                CreatedAt = DateTime.Now,
                LastModified = DateTime.Now,
                CreatedBy = currentUser.Id,
                ModifiedBy = currentUser.Id
            };
            settingService.AddSetting(setting);
        }
        else
        {
            setting.Subscribed = subscribed;
            setting.LastModified = DateTime.Now;
            setting.ModifiedBy = currentUser.Id;
        }

        await settingService.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Index()
    {
        var currentUser = await userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Unauthorized();
        }

        var scamCategories = await scamCategoryService.GetAllAsync();

        var settings = await settingService.GetAllSettingsByUserIdAsync(currentUser.Id);

        var viewModels = scamCategories.Select(sc => 
        {
            var userSetting = settings.FirstOrDefault(s => s.ScamCategoryId == sc.Id && s.UserId == currentUser!.Id);
    
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