using FraudFence.Data;
using FraudFence.EntityModels.Enums;
using FraudFence.EntityModels.Models;
using FraudFence.Web.Areas.Consumer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using System.Text.Json;
using FraudFence.Service;
using FraudFence.Service.Common;


//Amazon
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FraudFence.Web.Areas.Consumer.Controllers;

[Area("Consumer")]
public class ScamReportController(
    UserManager<ApplicationUser> userManager,
    ScamReportService _scamReportService,
    ScamCategoryService _scamCategoryService,
    ExternalAgencyService _externalAgencyService,
    AttachmentService _attachmentService,
    ScamReportAttachmentService _scamReportAttachmentService,
    PostAttachmentService _postAttachmentService,
    PostService _postService) : Controller
{
    
    private List<string> getValues()
    {
        List<string> values = new List<string>();

        var awsCredentials = FallbackCredentialsFactory.GetCredentials();
        var immutableCreds = awsCredentials.GetCredentials();

        values.Add(immutableCreds.AccessKey ?? "");
        values.Add(immutableCreds.SecretKey ?? "");
        values.Add(immutableCreds.Token ?? "");

        return values;    
        
    }
    

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Post(int id)
    {
        int scamReportId = id;
        
        var currentUser = await userManager.GetUserAsync(User);

        ScamReport? scamReport = await _scamReportService.GetScamReport(scamReportId);

        if (scamReport == null)
        {
            return NotFound();
        }

        if (scamReport.Status != ReportStatus.Verified)
        {
            return Unauthorized("Unverified scam report");
        }

        if (currentUser == null || currentUser.Id != scamReport.UserId)
        {
            return Unauthorized();
        }

        List<Post> checkPost = await _postService.GetPostsByScamReportId(scamReportId);

        if (checkPost.Count > 0)
        {
            return Conflict("Scam Report already exists");
        }

        Post post = new Post
        {
            UserId = currentUser.Id,
            Content = scamReport.Description,
            ScamReportId = scamReportId,
            CreatedAt = DateTime.Now,
            ModifiedBy = currentUser.Id,
            CreatedBy = currentUser.Id,
            LastModified = DateTime.Now
        };
        
        _postService.AddPost(post);

        List<ScamReportAttachment> scamReportAttachments =
            await _scamReportAttachmentService.GetScamReportAttachmentByScamReportId(scamReportId);
        
        
        foreach (ScamReportAttachment scamReportAttachment in scamReportAttachments)
        {
            PostAttachment postAttachment = new PostAttachment
            {
                PostId = post.Id,
                AttachmentId = scamReportAttachment.AttachmentId,
                CreatedBy = currentUser.Id,
                ModifiedBy = currentUser.Id,
                LastModified = DateTime.Now
            };

            await _postAttachmentService.AddPostAttachmentAsync(postAttachment);
        }
        
        
        
        TempData["SuccessMessage"] = "Post request sent successfully.";

        return RedirectToAction(nameof(ViewAll));
    }
    
    // [HttpPost]
    // [ValidateAntiForgeryToken]
    // public async Task<IActionResult> Delete(int id)
    // {
    //     var currentUser = await userManager.GetUserAsync(User);
    //     
    //     ScamReport? scamReport = await context.ScamReports
    //         .IgnoreAutoIncludes()
    //         .SingleOrDefaultAsync(sr => sr.Id == id);
    //     
    //     if (scamReport == null)
    //     {
    //         return NotFound();
    //     }
    //
    //     if (scamReport.UserId != currentUser!.Id)
    //     {
    //         return Unauthorized();
    //     }
    //
    //     context.ScamReports.Remove(scamReport);
    //
    //     await context.SaveChangesAsync();
    //         
    //     TempData["SuccessMessage"] = "Scam report deleted successfully.";
    //
    //     return RedirectToAction(nameof(ViewAll));
    // }
    
    public async Task<IActionResult> ViewAll()
    {
        
        var currentUser = await userManager.GetUserAsync(User);

        var reports = await _scamReportService.GetScamReportsWithUserId(currentUser!.Id);

        var viewModel = reports.Select(r => new ViewAllReportViewModel
        {
            Id = r.Id,
            ScamCategoryName = r.ScamCategory.Name,
            Description = r.Description,
            ReporterEmail = r.ReporterEmail,
            ReporterName = r.ReporterName,
            FirstEncounteredOn = r.FirstEncounteredOn,
            Status = r.Status,
            IsPosted = false
        }).ToList();
        
        var reportIds = viewModel.Select(r => r.Id).ToList();
        
        var relatedPosts = await _postService.GetScamReportIdWithStatus(reportIds);

        var postsByReportId = relatedPosts
            .GroupBy(p => p.ScamReportId)
            .ToDictionary(g => g.Key, g => g.Select(p => p.Status).ToList());

        foreach (var r in viewModel)
        {
            if (postsByReportId.TryGetValue(r.Id, out var postStatuses))
            {
                r.IsPosted = true;

                int statusValue = postStatuses.First();
                r.PostStatus = (FraudFence.EntityModels.Enums.PostStatus)statusValue;
            }
            else
            {
                r.IsPosted = false;
                r.PostStatus = null;
            }
        }

        return View(viewModel);
    }

    
    [HttpGet]
    public async Task<IActionResult> Submit()
    {
        List<ScamCategory> scamCategories = await _scamCategoryService.GetAllAsync();
        List<ExternalAgency> externalAgencies = await _externalAgencyService.GetAllAsync();

        SubmitReportViewModel submitReportViewModel = new SubmitReportViewModel();
        
        
        submitReportViewModel.FirstEncounteredOn = DateTime.Today;
        submitReportViewModel.ScamCategories = scamCategories.Select(category => new SelectListItem() { Text = category.Name, Value = category.Id.ToString() }).ToList();
        submitReportViewModel.ExternalAgencies = externalAgencies.Select(externalAgency => new SelectListItem() { Text = externalAgency.Name, Value = externalAgency.Id.ToString() }).ToList();
        
        return View(submitReportViewModel);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitReportViewModel submitReportViewModel)
    {
        if (!ModelState.IsValid)
        {
            List<ScamCategory> scamCategories = await _scamCategoryService.GetAllAsync();
            List<ExternalAgency> externalAgencies = await _externalAgencyService.GetAllAsync();
            
            submitReportViewModel.ScamCategories = scamCategories.Select(category => new SelectListItem() { Text = category.Name, Value = category.Id.ToString() }).ToList();
            submitReportViewModel.ExternalAgencies = externalAgencies.Select(externalAgency => new SelectListItem() { Text = externalAgency.Name, Value = externalAgency.Id.ToString() }).ToList();
            
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                foreach (var error in state.Errors)
                {
                    Console.WriteLine($"Model error on '{key}': {error.ErrorMessage}");
                }
            }
            
            return View(submitReportViewModel);
        }
        
        var currentUser = await userManager.GetUserAsync(User);
        
        if (User.Identity?.IsAuthenticated == true)
        {
            submitReportViewModel.ReporterName = currentUser.Name;
            submitReportViewModel.ReporterEmail = currentUser.Email;
            
            Console.Write(submitReportViewModel.ReporterName + " " + submitReportViewModel.ReporterEmail);
        }

        
        
        ScamReport report = new ScamReport
        {
            ScamCategoryId = submitReportViewModel.ScamCategoryId,
            ExternalAgencyId = submitReportViewModel.ExternalAgencyId,
            FirstEncounteredOn = submitReportViewModel.FirstEncounteredOn,
            Description = submitReportViewModel.Description,
            ReporterName = submitReportViewModel.ReporterName,
            ReporterEmail = submitReportViewModel.ReporterEmail,
            UserId = currentUser.Id,
            CreatedAt = DateTime.Now,
            LastModified = DateTime.Now,
            CreatedBy = User.Identity?.IsAuthenticated == true ? currentUser.Id : null,
            ModifiedBy = User.Identity?.IsAuthenticated == true ? currentUser.Id : null,
            DynamicData = "{}",
            Status = ReportStatus.Submitted
        };

        
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
        IConfigurationRoot configure = builder.Build();
        
        String bucketName = configure["S3BucketName"];
        
        
        
        List<string> values = getValues();
        var awsS3client = new AmazonS3Client(values[0], values[1], values[2], RegionEndpoint.USEast1);
        
        
        List<Attachment> attachments = new List<Attachment>();
        List<ScamReportAttachment> scamReportAttachments = new List<ScamReportAttachment>();
        
        foreach(var image in submitReportViewModel.Attachments)
        {
            if (image.Length <= 0)
            {
                return BadRequest("Empty image");
            }
            else if (image.Length > 10485760)
            {
                return BadRequest("Your file size is not more than 10 MB.");
            }
            else if (image.ContentType.ToLower() != "image/png" && image.ContentType.ToLower() != "image/jpeg"
                                                                && image.ContentType.ToLower() != "image/gif")
            {
                return BadRequest("Image format not valid");
            }
            
            
            try
            {
                String fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                PutObjectRequest uploadRequest = new PutObjectRequest
                {
                    InputStream = image.OpenReadStream(),
                    BucketName = bucketName,
                    Key = $"images/{fileName}",
                    CannedACL = S3CannedACL.PublicRead
                };
                await awsS3client.PutObjectAsync(uploadRequest);
                
                Attachment attachment = new Attachment
                {
                    FileName = image.FileName,
                    ContentType = image.ContentType,
                    BucketName = bucketName,
                    Link =  "https://"+ bucketName + ".s3.amazonaws.com/images/"+ fileName,
                };

                attachments.Add(attachment);
            }
            catch (Exception ex)
            {
                return BadRequest("Image upload to S3 error: " + ex.Message);
            }
            
        }
        
        await _scamReportService.AddScamReport(report);
        
        foreach (var attachment in attachments)
        {
            await _attachmentService.AddAttachmentAsync(attachment);

            scamReportAttachments.Add(new ScamReportAttachment
            {
                ScamReportId = report.Id,
                AttachmentId = attachment.Id
            });
        }
        
        await _scamReportAttachmentService.AddScamReportAttachmentRange(scamReportAttachments);
        
        
        TempData["Success"] = "Scam report submitted successfully!";
        return RedirectToAction("Submit");
    }
}