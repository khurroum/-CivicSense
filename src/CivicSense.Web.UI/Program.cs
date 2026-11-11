
using CivicSense.DataAccess;
using CivicSense.Facade.Implementation;
using CivicSense.Facade.Interface;
using CivicSense.Web.UI;
using CivicSense.Web.UI.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC and CSRF protection
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(
        new AutoValidateAntiforgeryTokenAttribute());
});

// Global exception handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// SQL Server
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection is missing.");

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString));

// DataAccess
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

// New Account Facade
builder.Services.AddScoped<
    IAccountService, AccountService>();

// Cookie validation
builder.Services.AddScoped<CustomCookieEvents>();

// Custom cookie authentication without Identity
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

        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath =
            "/Account/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromMinutes(30);

        options.SlidingExpiration = true;

        options.EventsType =
            typeof(CustomCookieEvents);
    });

// Require authentication unless an action
// explicitly allows anonymous access.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy =
        new Microsoft.AspNetCore.Authorization
            .AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});

// Antiforgery configuration for future AJAX requests
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();

// Existing centralized exception handler
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication MUST precede authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
