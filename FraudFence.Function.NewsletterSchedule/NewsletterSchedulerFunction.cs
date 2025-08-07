using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using FraudFence.Data;
using FraudFence.EntityModels.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FraudFence.Function.NewsletterSchedule;

public class NewsletterSchedulerFunction
{
    private readonly ApplicationDbContext _context;
    private readonly AmazonSQSClient _amazonSQSClient;
    private readonly string _queueUrl;

    public NewsletterSchedulerFunction()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        _context = new ApplicationDbContext(options);
        _amazonSQSClient = new AmazonSQSClient();
        _queueUrl = "https://sqs.us-east-1.amazonaws.com/211125322860/newsletter-email-queue";
    }

    public async Task<string> FunctionHandler()
    {
        var due = await _context.Newsletters
            .Include(n => n.Articles)
            .Where(n => n.SentAt == null && n.ScheduledAt <= DateTime.Now && !n.IsDisabled)
            .ToListAsync();

        foreach (var nl in due)
        {
            var categoryIds = nl.Articles.Select(a => a.ScamCategoryId).Distinct();
            var emails = await _context.Settings
                .Where(s => categoryIds.Contains(s.ScamCategoryId) && s.Subscribed)
                .Select(s => s.User.Email!)
                .ToArrayAsync();

            var body = GenerateEmailBody(nl);

            foreach (var email in emails)
            {
                await _amazonSQSClient.SendMessageAsync(new SendMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MessageBody = JsonSerializer.Serialize(
                        new { To = email, nl.Subject, Body = body })
                });
            }

            nl.SentAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return $"Processed {due.Count} newsletters";
    }

    private static string GenerateEmailBody(Newsletter newsletter)
    {
        var body = new StringBuilder();

        body.AppendLine("<!DOCTYPE html>");
        body.AppendLine("<html lang=\"en\">");
        body.AppendLine("<head>");
        body.AppendLine("    <meta charset=\"UTF-8\">");
        body.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        body.AppendLine($"    <title>{newsletter.Subject}</title>");
        body.AppendLine("    <style>");
        body.AppendLine(GetEmailStyles());
        body.AppendLine("    </style>");
        body.AppendLine("</head>");
        body.AppendLine("<body>");

        // Email container
        body.AppendLine("    <div class=\"email-container\">");

        // Header
        body.AppendLine("        <div class=\"header\">");
        body.AppendLine("            <div class=\"logo\">");
        body.AppendLine("                <span class=\"shield-icon\">🛡️</span>");
        body.AppendLine("                <h1>FraudFence</h1>");
        body.AppendLine("            </div>");
        body.AppendLine("            <div class=\"tagline\">Protecting You from Scams</div>");
        body.AppendLine("        </div>");

        // Newsletter content
        body.AppendLine("        <div class=\"content\">");
        body.AppendLine($"            <h2 class=\"newsletter-title\">{newsletter.Subject}</h2>");

        // Intro text
        if (!string.IsNullOrEmpty(newsletter.IntroText))
        {
            body.AppendLine("            <div class=\"intro-text\">");
            body.AppendLine($"                <p>{newsletter.IntroText}</p>");
            body.AppendLine("            </div>");
        }

        // Articles section
        if (newsletter.Articles.Count != 0)
        {
            body.AppendLine("            <div class=\"articles-section\">");
            body.AppendLine("                <h3 class=\"section-title\">📰 Latest Scam Alerts</h3>");

            foreach (var article in newsletter.Articles)
            {
                body.AppendLine("                <div class=\"article\">");
                body.AppendLine("                    <div class=\"article-header\">");
                body.AppendLine($"                        <h4 class=\"article-title\">{article.Title}</h4>");
                body.AppendLine("                        <span class=\"danger-badge\">⚠️ ALERT</span>");
                body.AppendLine("                    </div>");
                body.AppendLine("                    <div class=\"article-content\">");
                body.AppendLine($"                        {article.Content}");
                body.AppendLine("                    </div>");
                body.AppendLine("                </div>");
            }

            body.AppendLine("            </div>");
        }

        // Footer
        body.AppendLine("        </div>");
        body.AppendLine("        <div class=\"footer\">");
        body.AppendLine("            <div class=\"footer-content\">");
        body.AppendLine("                <p><strong>Stay Protected!</strong></p>");
        body.AppendLine("                <p>This newsletter was sent to keep you informed about the latest scam threats.</p>");
        body.AppendLine("                <div class=\"social-links\">");
        body.AppendLine("                    <span>Follow us for more updates</span>");
        body.AppendLine("                </div>");
        body.AppendLine("            </div>");
        body.AppendLine("            <div class=\"unsubscribe\">");
        body.AppendLine("                <p><small>Don't want to receive these emails? <a href=\"#\">Unsubscribe</a></small></p>");
        body.AppendLine("            </div>");
        body.AppendLine("        </div>");
        body.AppendLine("    </div>");
        body.AppendLine("</body>");
        body.AppendLine("</html>");

        return body.ToString();
    }

    private static string GetEmailStyles()
    {
        return @"
                * {
                    margin: 0;
                    padding: 0;
                    box-sizing: border-box;
                }
                
                body {
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
                    line-height: 1.6;
                    color: #333;
                    background-color: #f8f9fa;
                }
                
                .email-container {
                    max-width: 600px;
                    margin: 0 auto;
                    background: white;
                    border-radius: 12px;
                    overflow: hidden;
                    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                }
                
                .header {
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    color: white;
                    padding: 30px 40px;
                    text-align: center;
                }
                
                .logo {
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    gap: 10px;
                    margin-bottom: 10px;
                }
                
                .shield-icon {
                    font-size: 32px;
                }
                
                .logo h1 {
                    font-size: 28px;
                    font-weight: 700;
                    margin: 0;
                }
                
                .tagline {
                    font-size: 14px;
                    opacity: 0.9;
                    font-style: italic;
                }
                
                .content {
                    padding: 40px;
                }
                
                .newsletter-title {
                    color: #2d3748;
                    font-size: 24px;
                    font-weight: 700;
                    margin-bottom: 20px;
                    text-align: center;
                    border-bottom: 2px solid #e2e8f0;
                    padding-bottom: 15px;
                }
                
                .intro-text {
                    background: #f7fafc;
                    border-left: 4px solid #667eea;
                    padding: 20px;
                    margin-bottom: 30px;
                    border-radius: 0 8px 8px 0;
                }
                
                .intro-text p {
                    margin: 0;
                    color: #4a5568;
                    font-size: 16px;
                }
                
                .articles-section {
                    margin-top: 30px;
                }
                
                .section-title {
                    color: #2d3748;
                    font-size: 20px;
                    font-weight: 600;
                    margin-bottom: 25px;
                    padding-bottom: 10px;
                    border-bottom: 1px solid #e2e8f0;
                }
                
                .article {
                    background: #fff;
                    border: 1px solid #e2e8f0;
                    border-radius: 8px;
                    margin-bottom: 20px;
                    overflow: hidden;
                    transition: transform 0.2s ease;
                }
                
                .article:hover {
                    transform: translateY(-2px);
                    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
                }
                
                .article-header {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    padding: 20px 20px 0 20px;
                }
                
                .article-title {
                    color: #2d3748;
                    font-size: 18px;
                    font-weight: 600;
                    margin: 0;
                    flex: 1;
                }
                
                .danger-badge {
                    background: #fed7d7;
                    color: #c53030;
                    padding: 4px 8px;
                    border-radius: 4px;
                    font-size: 12px;
                    font-weight: 600;
                    margin-left: 10px;
                }
                
                .article-content {
                    padding: 20px;
                    color: #4a5568;
                    line-height: 1.7;
                }
                
                .article-content p {
                    margin-bottom: 15px;
                }
                
                .article-content h1,
                .article-content h2,
                .article-content h3,
                .article-content h4,
                .article-content h5,
                .article-content h6 {
                    color: #2d3748;
                    margin-bottom: 10px;
                    margin-top: 20px;
                }
                
                .footer {
                    background: #2d3748;
                    color: white;
                    padding: 30px 40px;
                }
                
                .footer-content {
                    text-align: center;
                    margin-bottom: 20px;
                }
                
                .footer-content p {
                    margin-bottom: 10px;
                }
                
                .social-links {
                    margin-top: 15px;
                    font-size: 14px;
                    opacity: 0.9;
                }
                
                .unsubscribe {
                    text-align: center;
                    padding-top: 20px;
                    border-top: 1px solid #4a5568;
                }
                
                .unsubscribe a {
                    color: #81c8ff;
                    text-decoration: none;
                }
                
                .unsubscribe a:hover {
                    text-decoration: underline;
                }
                
                @media (max-width: 600px) {
                    .email-container {
                        margin: 0;
                        border-radius: 0;
                    }
                    
                    .header,
                    .content,
                    .footer {
                        padding: 20px;
                    }
                    
                    .logo h1 {
                        font-size: 24px;
                    }
                    
                    .newsletter-title {
                        font-size: 20px;
                    }
                    
                    .article-header {
                        flex-direction: column;
                        align-items: flex-start;
                        gap: 10px;
                    }
                    
                    .danger-badge {
                        margin-left: 0;
                    }
                }
            ";
    }
}
