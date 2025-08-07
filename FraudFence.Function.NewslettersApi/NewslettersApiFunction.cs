using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FraudFence.Data;
using FraudFence.EntityModels.Dto.Newsletter;
using FraudFence.EntityModels.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FraudFence.Function.NewslettersApi;

public class NewslettersApiFunction
{
    private readonly ApplicationDbContext _context;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NewslettersApiFunction()
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionString");
        _context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs).Options);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest req)
    {
        return req.RequestContext.Http.Method switch
        {
            "GET" when req.RawPath == "/newsletters" => await GetAll(),
            "GET" => await Get(int.Parse(req.PathParameters["id"])),
            "POST" when req.RawPath == "/newsletters" => await Create(req.Body),
            "PUT" => await Update(int.Parse(req.PathParameters["id"]), req.Body),
            "DELETE" => await Delete(int.Parse(req.PathParameters["id"])),
            _ => new APIGatewayHttpApiV2ProxyResponse { StatusCode = 405 }
        };
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> GetAll()
    {
        var newsletters = await _context.Newsletters
            .Where(n => !n.IsDisabled || n.SentAt != null)
            .OrderBy(n => n.ScheduledAt)
            .Select(n => new NewsletterDTO(n.Id, n.Subject, n.IntroText, n.ScheduledAt, n.SentAt, n.IsDisabled))
            .ToListAsync();

        return Json200(newsletters);
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> Get(int id)
    {
        var newsletter = await _context.Newsletters
            .Include(n => n.Articles)
            .ThenInclude(a => a.ScamCategory)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (newsletter == null) return NotFound();

        var dto = new
        {
            newsletter.Id,
            newsletter.Subject,
            newsletter.IntroText,
            newsletter.ScheduledAt,
            newsletter.SentAt,
            newsletter.IsDisabled,
            Articles = newsletter.Articles.Select(a => new
            {
                a.Id,
                a.Title,
                CategoryName = a.ScamCategory.Name,
                a.IsDisabled
            }).ToList()
        };

        return Json200(dto);
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> Create(string body)
    {
        var dto = JsonSerializer.Deserialize<CreateNewsletterDTO>(body, JsonOptions);

        var newsletter = new Newsletter
        {
            Subject = dto!.Subject,
            IntroText = dto.IntroText,
            ScheduledAt = dto.ScheduledAt,
            CreatedAt = DateTime.Now,
            LastModified = DateTime.Now
        };

        var articles = await _context.Articles
            .Where(a => dto.SelectedArticleIds.Contains(a.Id))
            .ToListAsync();

        foreach (var article in articles)
        {
            newsletter.Articles.Add(article);
        }

        _context.Newsletters.Add(newsletter);
        await _context.SaveChangesAsync();
        return Created(newsletter.Id);
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> Update(int id, string body)
    {
        var dto = JsonSerializer.Deserialize<UpdateNewsletterDTO>(body, JsonOptions)!;

        var newsletter = await _context.Newsletters
            .Include(n => n.Articles)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (newsletter == null) return NotFound();

        newsletter.Subject = dto.Subject;
        newsletter.IntroText = dto.IntroText;
        newsletter.ScheduledAt = dto.ScheduledAt;
        newsletter.LastModified = DateTime.Now;

        newsletter.Articles.Clear();
        var articles = await _context.Articles
            .Where(a => dto.SelectedArticleIds.Contains(a.Id))
            .ToListAsync();

        foreach (var article in articles)
        {
            newsletter.Articles.Add(article);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> Delete(int id)
    {
        var newsletter = await _context.Newsletters.FindAsync(id);
        if (newsletter == null) return NotFound();

        if (newsletter.SentAt.HasValue)
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "Cannot delete a newsletter that has already been sent." })
            };
        }

        newsletter.IsDisabled = true;
        newsletter.LastModified = DateTime.Now;
        await _context.SaveChangesAsync();
        return Ok();
    }

    static APIGatewayHttpApiV2ProxyResponse Json200(object obj)
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(obj),
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
        };
    }

    static APIGatewayHttpApiV2ProxyResponse Ok()
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200
        };
    }

    static APIGatewayHttpApiV2ProxyResponse Created(int id)
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 201,
            Headers = new Dictionary<string, string> { ["Location"] = $"/newsletters/{id}" }
        };
    }

    static APIGatewayHttpApiV2ProxyResponse NotFound()
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 404
        };
    }
}
