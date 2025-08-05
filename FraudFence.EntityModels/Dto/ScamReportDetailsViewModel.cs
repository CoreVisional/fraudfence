using System;
using FraudFence.EntityModels.Enums;

namespace FraudFence.EntityModels.Dto
{
    public class ScamReportDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public DateTime DateSubmitted { get; set; }
        public ReportStatus Status { get; set; }
        public string Reviewer { get; set; } = string.Empty;
        public string InvestigationNotes { get; set; } = string.Empty;
    }
} 