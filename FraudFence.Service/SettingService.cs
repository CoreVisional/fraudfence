using FraudFence.Data;
using FraudFence.EntityModels.Models;
using FraudFence.Service.Common;
using Microsoft.EntityFrameworkCore;

namespace FraudFence.Service;

public class SettingService : BaseService<Setting>
{
    public SettingService(ApplicationDbContext context) : base(context)
    {
    }

    public override Task UpdateAsync(Setting entity)
    {
        throw new NotImplementedException();
    }

    public async Task<Setting?> GetSettingByUserIdAndScamCategoryId(string userId, int scamCategoryId)
    {
        return await _context.Settings
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ScamCategoryId == scamCategoryId);
    }

    public void AddSetting(Setting setting)
    {
        _context.Settings.Add(setting);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<Setting>> GetAllSettingsByUserIdAsync(string userId)
    {
        return await _context.Settings
            .Include(s => s.ScamCategory)
            .Where(s => s.UserId == userId)
            .IgnoreAutoIncludes()
            .ToListAsync();
    }
}