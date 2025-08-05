using FraudFence.EntityModels.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FraudFence.Interface.Common
{
    public interface IReviewerService
    {
        Task<List<ScamReport>> GetScamReportsAsync(string status);
        Task<ScamReport> GetScamReportDetailsAsync(int id);
        Task UpdateScamReportAsync(ScamReport report, List<int> reviewerIds, int? externalAgencyId, string investigationNotes, EntityModels.Enums.ReportStatus status);
    }
} 