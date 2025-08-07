using FraudFence.Data;
using FraudFence.EntityModels.Enums;
using FraudFence.EntityModels.Models;
using Microsoft.EntityFrameworkCore;

namespace FraudFence.Service.Common;

public class PostService : BaseService<Post>
{
    
    public PostService(ApplicationDbContext context) : base(context)
    {
    }


    public override Task UpdateAsync(Post entity)
    {
        throw new NotImplementedException();
    }

    public async Task<List<(int ScamReportId, int Status)>> GetScamReportIdWithStatus(List<int> scamReportIds)
    {
        return await _context.Posts
            .Where(p => scamReportIds.Contains(p.ScamReportId))
            .IgnoreAutoIncludes()
            .Select(p => new ValueTuple<int, int>(p.ScamReportId, p.Status))
            .ToListAsync();
    }

    public void AddPost(Post post)
    {
        _context.Posts.Add(post);
        _context.SaveChanges();
    }

    public async Task<List<Post>> GetPostsByScamReportId(int scamReportId)
    {
        return await _context.Posts
            .Where(p => p.ScamReportId == scamReportId)
            .IgnoreAutoIncludes()
            .ToListAsync();
    }

    public async Task<Post?> GetPostById(int id)
    {
        return await _context.Posts.IgnoreAutoIncludes().SingleOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Post?> GetPostCompleteById(int id)
    {
        return await _context.Posts
            .IgnoreAutoIncludes()
            .Include(p => p.ScamReport)
            .Include(p => p.Comments)
            .ThenInclude(c => c.User)
            .Include(p => p.PostAttachments)
            .ThenInclude(pa => pa.Attachment)
            .Include(p => p.User)
            .SingleOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Post>> GetAcceptedPosts()
    {
        return await _context.Posts
            .IgnoreQueryFilters()
            .IgnoreAutoIncludes()
            .Where(p => p.Status == (int)PostStatus.Accepted)
            .Include(p => p.ScamReport)
            .Include(p => p.Comments)
            .Include(p => p.PostAttachments)
            .ThenInclude(pa => pa.Attachment)
            .Include(p => p.User)
            .ToListAsync();
    }

    public async Task<List<Post>> GetMyPosts(string userId)
    {
        return await _context.Posts
            .IgnoreQueryFilters()
            .IgnoreAutoIncludes()
            .Where(p => p.UserId == userId)
            .Include(p => p.ScamReport)
            .Include(p => p.Comments)
            .Include(p => p.PostAttachments)
            .ThenInclude(pa => pa.Attachment)
            .Include(p => p.User)
            .ToListAsync();
    }

    public async Task UpdatePost(Post post)
    {
        _context.Posts.Update(post);

        await _context.SaveChangesAsync();
    }

    public async Task DeletePost(Post post)
    {
        _context.Posts.Remove(post);

        await _context.SaveChangesAsync();
    }
}