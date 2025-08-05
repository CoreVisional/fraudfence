using FraudFence.Data;
using FraudFence.EntityModels.Models;
using FraudFence.Service.Common;

namespace FraudFence.Service;

public class CommentService : BaseService<Comment>
{
    public CommentService(ApplicationDbContext context) : base(context)
    {
    }

    public override Task UpdateAsync(Comment entity)
    {
        throw new NotImplementedException();
    }

    public async Task AddCommentAsync(Comment comment)
    {
        _context.Comments.Add(comment);
       await _context.SaveChangesAsync();
    }
}