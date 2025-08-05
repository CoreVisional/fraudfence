using FraudFence.Data;
using FraudFence.EntityModels.Models;
using FraudFence.Interface.Common;
using FraudFence.Service;
using FraudFence.Service.Common;
using FraudFence.Web.Infrastructure;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-MY");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-MY");
CultureInfo.CurrentUICulture = new CultureInfo("en-MY");
CultureInfo.CurrentCulture = new CultureInfo("en-MY");

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<ApplicationDbContext>((sp, options) =>
    {
        var _auditInt = sp.GetRequiredService<AuditInterceptor>();

        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure(2)
        )
        .AddInterceptors(_auditInt);
#if DEBUG
        options.EnableSensitiveDataLogging();
#endif
    })
    .AddIdentity<ApplicationUser, IdentityRole<int>>(opts =>
    {
        opts.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.AccessDeniedPath = "/Home/Error/403";
});

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Service Registrations to DI
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<AuditInterceptor>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<EmailSettings>>().Value);
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ScamCategoryService>();
builder.Services.AddScoped<ExternalAgencyService>();
builder.Services.AddScoped<IReviewerService, ReviewerService>();
builder.Services.AddScoped<ScamReportService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<SettingService>();
builder.Services.AddScoped<AttachmentService>();
builder.Services.AddScoped<ScamReportAttachmentService>();
builder.Services.AddScoped<PostAttachmentService>();

builder.Services.AddScoped<ScamCategoryService>();
builder.Services.AddScoped<ArticleService>();
builder.Services.AddScoped<NewsletterService>();

builder.Services.AddHangfire(cfg => cfg.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

#if DEBUG
IdentityModelEventSource.ShowPII = true;
#endif

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    db.Database.Migrate();

    await IdentitySeeder.SeedAsync(scope.ServiceProvider, builder.Configuration);
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

app.UseHttpsRedirection();
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
