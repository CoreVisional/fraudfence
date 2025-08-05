using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FraudFence.Web.Areas.Consumer.Models;

public class SubmitReportViewModel
{
    [Required]
    public int ScamCategoryId { get; set; }

    public int? ExternalAgencyId { get; set; }

    [Required]
    public DateTime FirstEncounteredOn { get; set; }
    
    [Required]
    public string Description { get; set; } = "";
    
    public string ReporterName { get; set; } = "";
    
    public string ReporterEmail { get; set; } = "";

    public bool Subscribed { get; set; } = false;

    public List<IFormFile> Attachments { get; set; } = new();
    
    public List<SelectListItem> ScamCategories { get; set; } = new();
    public List<SelectListItem> ExternalAgencies { get; set; } = new();
}