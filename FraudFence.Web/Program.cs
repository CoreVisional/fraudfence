using FraudFence.Data;
using FraudFence.Interface.Common;
using FraudFence.Service;
using FraudFence.Service.Common;
using FraudFence.Web.Infrastructure;
using FraudFence.Web.Infrastructure.Api;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-MY");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-MY");
CultureInfo.CurrentUICulture = new CultureInfo("en-MY");
CultureInfo.CurrentCulture = new CultureInfo("en-MY");

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<ApplicationDbContext>((sp, options) =>
    {
        var auditInt = sp.GetRequiredService<AuditInterceptor>();

        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure(2)
        )
        .AddInterceptors(auditInt);
#if DEBUG
        options.EnableSensitiveDataLogging();
#endif
    });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Accounts/Login";
        options.LogoutPath = "/Accounts/Logout";
        options.AccessDeniedPath = "/Home/Error/403";
    });

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Service Registrations to DI
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<AuditInterceptor>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ScamCategoryService>();
builder.Services.AddScoped<ExternalAgencyService>();
builder.Services.AddScoped<IReviewerService, ReviewerService>();
builder.Services.AddScoped<ScamReportService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<SettingService>();
builder.Services.AddScoped<ArticleService>();
builder.Services.AddScoped<NewsletterService>();

builder.Services.AddHttpClient<ArticleApiClient>();
builder.Services.AddHttpClient("UsersApi");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    // Wait for the seeder to complete before continuing
    CognitoSeeder.SeedAsync(scope.ServiceProvider, builder.Configuration).Wait();
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error/500");
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
