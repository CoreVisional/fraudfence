using System;
using FraudFence.EntityModels.Enums;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FraudFence.Web.Areas.Reviewer.Models
{
    public class ScamReportDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime DateSubmitted { get; set; }
        public ReportStatus Status { get; set; }
        public string Reviewer { get; set; }
        public string InvestigationNotes { get; set; }
        public int? ExternalAgencyId { get; set; }
        public string? ExternalAgencyName { get; set; }
        public List<SelectListItem>? Agencies { get; set; }
        public List<int> SelectedReviewerIds { get; set; } = new();
        public List<SelectListItem> AllReviewers { get; set; } = new();
        public DateTime FirstEncounteredOn { get; set; }
    }
} 