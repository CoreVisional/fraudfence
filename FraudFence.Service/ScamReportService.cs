using FraudFence.Data;
using FraudFence.EntityModels.Models;
using FraudFence.Service.Common;
using Microsoft.EntityFrameworkCore;


namespace FraudFence.Service;

public class ScamReportService : BaseService<ScamReport>
{
    public ScamReportService(ApplicationDbContext context) : base(context)
    {
    }
    
    public override async Task UpdateAsync(ScamReport entity)
    {
        var inDb = await _context.ScamReports.FindAsync(entity.Id);
        if (inDb == null) return;
        inDb.Id = entity.Id;
        inDb.InvestigationNotes = entity.InvestigationNotes;
        inDb.ScamCategoryId = entity.ScamCategoryId;
        inDb.ExternalAgencyId = entity.ExternalAgencyId;
        inDb.GuestId = entity.GuestId;
        inDb.DynamicData = entity.DynamicData;
        inDb.Description = entity.Description;
        inDb.FirstEncounteredOn = entity.FirstEncounteredOn;
        inDb.ReporterEmail=entity.ReporterEmail;
        inDb.ReporterName = entity.ReporterName;
        inDb.Status = entity.Status;
        
        await _context.SaveChangesAsync();
    }

    public async Task AddScamReport(ScamReport entity)
    {
        _context.ScamReports.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ScamReport>> GetScamReportsWithUserId(string userId)
    {
        return await _context.ScamReports
            .Include(r => r.ScamCategory)
            .Where(r => r.UserId == userId)
            .IgnoreAutoIncludes()
            .ToListAsync();
    }

    public async Task<ScamReport?> GetScamReport(int scamReportId)
    {
        return await _context.ScamReports
            .IgnoreAutoIncludes()
            .Include(sr => sr.User)
            .FirstOrDefaultAsync(sr => sr.Id == scamReportId);
    }
    
    
    
}