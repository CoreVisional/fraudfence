using System.Security.Claims;
using FraudFence.Data;
using FraudFence.EntityModels.Enums;
using FraudFence.EntityModels.Models;
using FraudFence.Service;
using FraudFence.Service.Common;
using FraudFence.Web.Areas.Consumer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FraudFence.Web.Areas.Consumer.Controllers;

[Area("Consumer")]
[Authorize(Roles = "Consumer")]
public class PostController(UserManager<ApplicationUser> userManager, PostService postService, CommentService commentService) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(CommentViewModel commentViewModel)
    {
        
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(commentViewModel.Content))
        {
            return BadRequest("Comment content is required.");
        }

        int userId;
        try
        {
            userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }
        catch
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
        int userId = 0;

        try
        {
            userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }
        catch
        {
            return Unauthorized();
        }



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
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

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
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

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
        var currentUser = await userManager.GetUserAsync(User);
        
        Post? post = await postService.GetPostById(id);

        if (post == null)
        {
            return NotFound();
        }
        
        if (currentUser == null || post.UserId != currentUser!.Id)
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
        var currentUser = await userManager.GetUserAsync(User);
        
        Post? post = await postService.GetPostById(editPostViewModel.Id);
        
        if (post == null)
        {
            return NotFound();
        }

        if (currentUser == null || post.UserId != currentUser!.Id)
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
        var currentUser = await userManager.GetUserAsync(User);
        
        Post? post = await postService.GetPostById(id);
        
        
        if (post == null)
        {
            return NotFound();
        }

        if (currentUser == null || post.UserId != currentUser!.Id)
        {
            return Unauthorized();
        }

        await postService.DeletePost(post);
            
        TempData["SuccessMessage"] = "Post deleted successfully.";

        return RedirectToAction(nameof(MyPosts));
    }
    
    
}