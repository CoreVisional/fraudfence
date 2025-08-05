using System;
using FraudFence.EntityModels.Enums;

namespace FraudFence.Web.Areas.Reviewer.Models
{
    public class ScamReportListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime DateSubmitted { get; set; }
        public ReportStatus Status { get; set; }
        public string Reviewer { get; set; }
        public string? ExternalAgencyName { get; set; }
    }
}
