using FraudFence.Data;
using FraudFence.EntityModels.Models;
using FraudFence.Service.Common;
using Microsoft.EntityFrameworkCore;

namespace FraudFence.Service;

public class ScamReportAttachmentService : BaseService<ScamReportAttachment>
{
    public ScamReportAttachmentService(ApplicationDbContext context) : base(context)
    {
    }

    public override Task UpdateAsync(ScamReportAttachment entity)
    {
        throw new NotImplementedException();
    }

    public async Task AddScamReportAttachmentRange(List<ScamReportAttachment> attachments)
    {
        _context.ScamReportAttachments.AddRange(attachments);
        await _context.SaveChangesAsync();
    }
    
    public async Task<List<ScamReportAttachment>> GetScamReportAttachmentByScamReportId(int scamReportId)
    {
        return await _context.ScamReportAttachments
            .Where(p => p.ScamReportId == scamReportId)
            .Include(p => p.Attachment)
            .IgnoreAutoIncludes()
            .ToListAsync();
    }
}