using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FraudFence.Data;
using FraudFence.EntityModels.Dto.Article;
using FraudFence.EntityModels.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FraudFence.EntityModels.Dto.ScamReport;
using FraudFence.EntityModels.Enums;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FraudFence.Function.ScamReportApi;


public class ScamReportApiFunction
{
    private readonly ApplicationDbContext _context;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };


    public ScamReportApiFunction()
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionString");
        _context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs).Options);
    }
    
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest req)
    {
        
        
        return req.RequestContext.Http.Method switch
        {
            "GET" when req.RawPath.StartsWith("/scamReports/owner/") =>
                await GetReportsByUserId(int.Parse(req.PathParameters["userId"])),

            "GET" when req.RawPath.StartsWith("/scamReports/") =>
                await Get(int.Parse(req.PathParameters["id"])),

            "POST" when req.RawPath == "/scamReports" =>
                await Create(req.Body),

            _ => new APIGatewayHttpApiV2ProxyResponse { StatusCode = 405 }
        };
    }
    
    
    private async Task<APIGatewayHttpApiV2ProxyResponse> GetReportsByUserId(int id)
    {
        List<ScamReport> scamReports = await _context.ScamReports
            .Include(r => r.ScamCategory)
            .Where(r => r.UserId == id)
            .IgnoreAutoIncludes()
            .ToListAsync();
        
        List<ScamReportDTO> scamReportDTOs = [];

        foreach (ScamReport scamReport in scamReports)
        {
            var dto = new ScamReportDTO
            {
                Id = scamReport.Id,
                ScamCategoryId = scamReport.ScamCategoryId,
                ExternalAgencyId = scamReport.ExternalAgencyId,
                GuestId = scamReport.GuestId,
                UserId = scamReport.UserId,
                FirstEncounteredOn = scamReport.FirstEncounteredOn,
                Description = scamReport.Description,
                ReporterName = scamReport.ReporterName,
                ReporterEmail = scamReport.ReporterEmail,
                Status = scamReport.Status,
                DynamicData = scamReport.DynamicData,
                ScamCategory = new ScamCategoryDTO
                {
                    Id = scamReport.ScamCategory.Id,
                    Name = scamReport.ScamCategory.Name,
                }
            };
            scamReportDTOs.Add(dto);
        }
        
        
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(scamReportDTOs),
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
        };
    }
    
    private async Task<APIGatewayHttpApiV2ProxyResponse> Get(int id)
    {
        var scamReport = await _context.ScamReports.IgnoreAutoIncludes().Include(sr => sr.User).FirstOrDefaultAsync(sr => sr.Id == id);
        if (scamReport == null) return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 404 };

        var dto = new ScamReportDTO
        {
            ScamCategoryId = scamReport.ScamCategoryId,
            ExternalAgencyId = scamReport.ExternalAgencyId,
            GuestId = scamReport.GuestId,
            UserId = scamReport.UserId,
            FirstEncounteredOn = scamReport.FirstEncounteredOn,
            Description = scamReport.Description,
            ReporterName = scamReport.ReporterName,
            ReporterEmail = scamReport.ReporterEmail,
            DynamicData = scamReport.DynamicData,
            Status = scamReport.Status
        };
        
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(dto),
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
        };
    }
    
    
    private async Task<APIGatewayHttpApiV2ProxyResponse> Create(string body)
    {
        var dto = JsonSerializer.Deserialize<CreateScamReportDTO>(body, JsonOptions);

        ScamReport scamReport = new ScamReport()
        {
            ScamCategoryId = dto.ScamCategoryId,
            ExternalAgencyId = dto.ExternalAgencyId,
            FirstEncounteredOn = dto.FirstEncounteredOn,
            Description = dto.Description,
            ReporterName = dto.ReporterName,
            ReporterEmail = dto.ReporterEmail,
            UserId = dto?.UserId,
            CreatedAt = DateTime.Now,
            LastModified = DateTime.Now,
            CreatedBy = dto?.CreatedBy,
            ModifiedBy = dto?.ModifiedBy,
            DynamicData = "{}",
            Status = ReportStatus.Submitted
        };
        
        _context.ScamReports.Add(scamReport);
        
        await _context.SaveChangesAsync();
        
        var returnDto = new ScamReportDTO
        {
            Id = scamReport.Id,
            ScamCategoryId = scamReport.ScamCategoryId,
            ExternalAgencyId = scamReport.ExternalAgencyId,
            GuestId = scamReport.GuestId,
            UserId = scamReport.UserId,
            FirstEncounteredOn = scamReport.FirstEncounteredOn,
            Description = scamReport.Description,
            ReporterName = scamReport.ReporterName,
            ReporterEmail = scamReport.ReporterEmail,
            DynamicData = scamReport.DynamicData,
        };
        

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 201,
            Body = JsonSerializer.Serialize(returnDto),
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
        };
    }
    
    
    
}