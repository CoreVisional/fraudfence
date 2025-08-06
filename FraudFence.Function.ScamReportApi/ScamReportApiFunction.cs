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
            "GET" when req.RawPath == "/scamReports" => await GetAll(),
            "GET" => await Get(int.Parse(req.PathParameters["id"])),
            "POST" when req.RawPath == "/scamReports" => await Create(req.Body),
            "PUT" => await Update(int.Parse(req.PathParameters["id"]), req.Body),
            "DELETE" => await Delete(int.Parse(req.PathParameters["id"])),
            _ => new APIGatewayHttpApiV2ProxyResponse { StatusCode = 405 }
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

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 201,
            Headers = new Dictionary<string, string> { ["Location"] = $"/scamReports/{scamReport.ScamCategoryId}" }
        };
    }
    
    
    
}