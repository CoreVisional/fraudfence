using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FraudFence.EntityModels.Enums;
using FraudFence.Web.Areas.Reviewer.Models;
using Microsoft.EntityFrameworkCore;
using FraudFence.Data;

namespace FraudFence.Web.Areas.Reviewer.Controllers
{
    [Area("Reviewer")]
    [Authorize(Roles = "Reviewer")]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string status)
        {
            var query = _context.Posts
                .IgnoreAutoIncludes()
                .AsNoTracking();

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<PostStatus>(status, out var statusEnum))
                {
                    int statusValue = (int)statusEnum;
                    query = query.Where(p => p.Status == statusValue);
                }
            }

            var posts = await query
                .Include(p => p.User)
                .Select(p => new PostListViewModel
                {
                    Id = p.Id,
                    Content = p.Content.Length > 80 ? p.Content.Substring(0, 80) + "..." : p.Content,
                    Author = p.User.Name,
                    Date = p.CreatedAt,
                    Status = ((PostStatus)p.Status).ToString()
                })
                .ToListAsync();
            ViewBag.SelectedStatus = status;
            return View(posts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
                .IgnoreAutoIncludes()
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.PostAttachments)
                    .ThenInclude(pa => pa.Attachment)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            var viewModel = new PostDetailsViewModel
            {
                Id = post.Id,
                Content = post.Content,
                Author = post.User.Name,
                Date = post.CreatedAt,
                ImageLinks = post.PostAttachments.Select(pa => pa.Attachment.Link).ToList(),
                Status = (PostStatus)post.Status
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(PostDetailsViewModel model)
        {
            var post = await _context.Posts.IgnoreAutoIncludes().FirstOrDefaultAsync(p => p.Id == model.Id);
            if (post == null)
            {
                return NotFound();
            }
            post.Status = (int)model.Status;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
