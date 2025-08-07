using System.Security.Claims;
using FraudFence.EntityModels.Models;
using FraudFence.Service;
using FraudFence.Service.Common;
using FraudFence.Web.Areas.Consumer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FraudFence.Web.Areas.Consumer.Controllers;

[Area("Consumer")]
[Authorize(Roles = "Consumer")]
public class PostController(PostService postService, CommentService commentService) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(CommentViewModel commentViewModel)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(commentViewModel.Content))
        {
            return BadRequest("Comment content is required.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var post = await postService.GetPostById(commentViewModel.PostId);
        if (post == null)
        {
            return NotFound();
        }

        var comment = new Comment
        {
            UserId = userId,
            PostId = commentViewModel.PostId,
            Content = commentViewModel.Content,
            CreatedAt = DateTime.UtcNow
        };

        await commentService.AddCommentAsync(comment);

        TempData["SuccessMessage"] = "Commented successfully.";
        return RedirectToAction(nameof(ViewComment), new { id = commentViewModel.PostId });
    }

    [HttpGet]
    public async Task<IActionResult> ViewComment(int id)
    {
        var post = await postService.GetPostCompleteById(id);
        
        CommentViewModel model = new CommentViewModel
        {
            Post = post
        };
        
        return View(model);
    }
    
    [HttpGet]
    public async Task<IActionResult> MyFeed()
    {
        var posts = await postService.GetAcceptedPosts();

        var model = new PostViewModel
        {
            Posts = posts
        };
        
        return View(model);
    }
    
    [HttpGet]
    public async Task<IActionResult> MyPosts()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var posts = await postService.GetMyPosts(userId);

        var model = new PostViewModel
        {
            Posts = posts
        };
        
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Post? post = await postService.GetPostById(id);

        if (post == null)
        {
            return NotFound();
        }
        
        if (string.IsNullOrEmpty(currentUserId) || post.UserId != currentUserId)
        {
            return Unauthorized();
        }

        EditPostViewModel vm = new EditPostViewModel
        {
            Post = post,
            Content = post.Content
        };
        
        return View(vm);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditPostViewModel editPostViewModel)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Post? post = await postService.GetPostById(editPostViewModel.Id);
        
        if (post == null)
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(currentUserId) || post.UserId != currentUserId)
        {
            return Unauthorized();
        }
        
        post.Content = editPostViewModel.Content;

        await postService.UpdatePost(post);
            
        TempData["SuccessMessage"] = "Post updated successfully.";

        return RedirectToAction(nameof(MyPosts));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Post? post = await postService.GetPostById(id);
        
        if (post == null)
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(currentUserId) || post.UserId != currentUserId)
        {
            return Unauthorized();
        }

        await postService.DeletePost(post);
            
        TempData["SuccessMessage"] = "Post deleted successfully.";

        return RedirectToAction(nameof(MyPosts));
    }
}