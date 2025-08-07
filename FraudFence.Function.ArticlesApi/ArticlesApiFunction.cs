using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FraudFence.Data;
using FraudFence.EntityModels.Dto.Article;
using FraudFence.EntityModels.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FraudFence.Function.ArticlesApi;

public class ArticlesApiFunction
{
    private readonly ApplicationDbContext _context;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ArticlesApiFunction()
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionString");
        _context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs).Options);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest req)
    {
        return req.RequestContext.Http.Method switch
        {
            "GET" when req.RawPath == "/articles" => await GetAll(),
            "GET" => await Get(int.Parse(req.PathParameters["id"])),
            "POST" when req.RawPath == "/articles" => await Create(req.Body),
            "PUT" => await Update(int.Parse(req.PathParameters["id"]), req.Body),
            "DELETE" => await Delete(int.Parse(req.PathParameters["id"])),
            _ => new APIGatewayHttpApiV2ProxyResponse { StatusCode = 405 }
        };
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> GetAll()
    {
        var articles = await _context.Articles.Include(a => a.ScamCategory)
            .Select(a => new ArticleDTO(a.Id, a.Title, a.ScamCategory.Name, a.Content, a.ScamCategoryId, a.CreatedAt, a.LastModified))
            .ToListAsync();

        return Json200(articles);
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> Get(int id)
    {
        var article = await _context.Articles.Include(a => a.ScamCategory).FirstOrDefaultAsync(a => a.Id == id);
        if (article == null) return NotFound();

        var dto = new ArticleDTO(article.Id, article.Title, article.ScamCategory.Name, article.Content, article.ScamCategoryId, article.CreatedAt, article.LastModified);
        return Json200(dto);
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> Create(string body)
    {
        var dto = JsonSerializer.Deserialize<CreateArticleDTO>(body, JsonOptions);

        var article = new Article
        {
            Title = dto!.Title,
            Content = dto.Content,
            ScamCategoryId = dto.ScamCategoryId,
            UserId = dto.UserId,
            CreatedAt = DateTime.Now,
            LastModified = DateTime.Now
        };

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        return Created(article.Id);
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> Update(int id, string body)
    {
        var dto = JsonSerializer.Deserialize<CreateArticleDTO>(body, JsonOptions)!;

        var article = await _context.Articles.FindAsync(id);
        if (article == null) return NotFound();

        article.Title = dto.Title;
        article.Content = dto.Content;
        article.ScamCategoryId = dto.ScamCategoryId;
        article.LastModified = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok();
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> Delete(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null) return NotFound();
        _context.Articles.Remove(article);
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
            Headers = new Dictionary<string, string> { ["Location"] = $"/articles/{id}" }
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
