using FraudFence.Data;
using FraudFence.EntityModels.Models;
using FraudFence.Service.Common;

namespace FraudFence.Service;

public class PostAttachmentService : BaseService<PostAttachment>
{
    public PostAttachmentService(ApplicationDbContext context) : base(context)
    {
    }

    public override Task UpdateAsync(PostAttachment entity)
    {
        throw new NotImplementedException();
    }
    
    public async Task AddPostAttachmentAsync(PostAttachment postAttachment)
    {
        _context.PostAttachments.Add(postAttachment);
        await _context.SaveChangesAsync();
    }
}