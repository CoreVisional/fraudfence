using FraudFence.Data;
using FraudFence.EntityModels.Models;
using FraudFence.Interface.Common;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FraudFence.Service.Common
{
    public class ReviewerService : IReviewerService
    {
        private readonly ApplicationDbContext _context;
        public ReviewerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ScamReport>> GetScamReportsAsync(string status)
        {
            var query = _context.ScamReports
                .AsNoTracking()
                .IgnoreAutoIncludes()
                .Include(r => r.Reviewers)
                .Include(r => r.ExternalAgency)
                .Where(r =>
                    r.Status != EntityModels.Enums.ReportStatus.Draft &&
                    r.Status != EntityModels.Enums.ReportStatus.Withdrawn &&
                    !r.IsDisabled
                );
            if (!string.IsNullOrEmpty(status))
            {
                if (System.Enum.TryParse<EntityModels.Enums.ReportStatus>(status, out var statusEnum))
                {
                    query = query.Where(r => r.Status == statusEnum);
                }
            }
            return await query.ToListAsync();
        }

        public async Task<ScamReport> GetScamReportDetailsAsync(int id)
        {
            return await _context.ScamReports
                .AsNoTracking()
                .IgnoreAutoIncludes()
                .Include(r => r.Reviewers)
                .Include(r => r.ExternalAgency)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDisabled);
        }

        public async Task UpdateScamReportAsync(ScamReport report, List<int> reviewerIds, int? externalAgencyId, string investigationNotes, EntityModels.Enums.ReportStatus status)
        {
            var dbReport = await _context.ScamReports
                .IgnoreAutoIncludes()
                .Include(r => r.Reviewers)
                .Include(r => r.ExternalAgency)
                .FirstOrDefaultAsync(r => r.Id == report.Id && !r.IsDisabled);
            if (dbReport == null) return;
            dbReport.InvestigationNotes = investigationNotes;
            dbReport.Status = status;
            // Update reviewers
            if (reviewerIds != null)
            {
                var reviewersToRemove = dbReport.Reviewers.Where(u => !reviewerIds.Contains(u.Id)).ToList();
                foreach (var reviewer in reviewersToRemove)
                {
                    dbReport.Reviewers.Remove(reviewer);
                }
                var currentReviewerIds = dbReport.Reviewers.Select(u => u.Id).ToHashSet();
                var reviewersToAdd = reviewerIds.Where(id => !currentReviewerIds.Contains(id)).ToList();
                if (reviewersToAdd.Any())
                {
                    var usersToAdd = await _context.Users.Where(u => reviewersToAdd.Contains(u.Id)).ToListAsync();
                    foreach (var user in usersToAdd)
                    {
                        dbReport.Reviewers.Add(user);
                    }
                }
            }
            // Assign external agency if selected
            if (externalAgencyId.HasValue)
            {
                dbReport.ExternalAgencyId = externalAgencyId;
            }
            await _context.SaveChangesAsync();
        }
    }
} 