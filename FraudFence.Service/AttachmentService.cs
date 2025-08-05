using FraudFence.Data;
using FraudFence.EntityModels.Models;
using FraudFence.Service.Common;

namespace FraudFence.Service;


public class AttachmentService : BaseService<Attachment>
{
    public AttachmentService(ApplicationDbContext context) : base(context)
    {
    }


    public override Task UpdateAsync(Attachment entity)
    {
        throw new NotImplementedException();
    }
    
    public async Task AddAttachmentAsync(Attachment attachment)
    {
        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();
    }
}