using CivicSense.Entities;
using CivicSense.DataAccess;
using CivicSense.DataAccess.Interface;
using CivicSense.DataAccess.Implementation;
using CivicSense.Facade.Implementation;
using CivicSense.Facade.Interface;
using CivicSense.Web.UI;
using CivicSense.Web.UI.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC and antiforgery protection
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(
        new AutoValidateAntiforgeryTokenAttribute());
});

// Global exception handling
builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<
    GlobalExceptionHandler>();

// SQL Server
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection is missing.");

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString));

// DataAccess services
builder.Services.AddDataAccessServices();

// Existing Facade services
builder.Services.AddScoped<
    ICategoryService, CategoryService>();

builder.Services.AddScoped<
    IIssueService, IssueService>();

builder.Services.AddScoped<
    IIssueWorkflowService, IssueWorkflowService>();

builder.Services.AddScoped<
    IIssueCommentService, IssueCommentService>();

builder.Services.AddScoped<
    IIssueConfirmationService, IssueConfirmationService>();

builder.Services.AddScoped<
    IFeedbackService, FeedbackService>();

builder.Services.AddScoped<
    INotificationService, NotificationService>();

builder.Services.AddScoped<
    IAIAnalysisService, AIAnalysisService>();

builder.Services.AddScoped<
    IServiceProviderService, ServiceProviderService>();

builder.Services.AddScoped<
    IServiceRequestService, ServiceRequestService>();

builder.Services.AddScoped<
    IPaymentService, PaymentService>();

builder.Services.AddScoped<
    IAccountService, AccountService>();

// Dynamic permission foundation
builder.Services.AddScoped<
    IPermissionRepository, PermissionRepository>();

builder.Services.AddScoped<
    IPermissionService, PermissionService>();

builder.Services.AddScoped<
    IAuthorizationHandler, PermissionAuthorizationHandler>();

// NEW: Access administration
builder.Services.AddScoped<
    IAccessAdminRepository, AccessAdminRepository>();

builder.Services.AddScoped<
    IAccessAdminService, AccessAdminService>();

// Cookie validation
builder.Services.AddScoped<CustomCookieEvents>();

// Cookie authentication
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "CivicSense.Auth";

        options.Cookie.HttpOnly = true;

        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;

        options.Cookie.SameSite =
            SameSiteMode.Lax;

        options.LoginPath =
            "/Account/Login";

        options.AccessDeniedPath =
            "/Account/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromMinutes(30);

        options.SlidingExpiration = true;

        options.EventsType =
            typeof(CustomCookieEvents);
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        PermissionCodes.ManageCategories,
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.AddRequirements(
                new PermissionRequirement(
                    PermissionCodes.ManageCategories));
        });

    // NEW: Protected access management.
    options.AddPolicy(
        PermissionCodes.ManageAccess,
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireRole("Admin");

            policy.AddRequirements(
                new PermissionRequirement(
                    PermissionCodes.ManageAccess));
        });

    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});

// Antiforgery configuration
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();