using EmployeeAccessSystem.Filters;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Services;
using EmployeeAccessSystem.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Filter
builder.Services.AddScoped<PermissionFilter>();

builder.Services.AddControllersWithViews(options =>
{
    // Global permission filter
    options.Filters.AddService<PermissionFilter>();

    // Global antiforgery token for POST forms
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Database connection
builder.Services.AddSingleton<ICoreDbConnection, CoreDbConnection>();

// Repositories

builder.Services.AddScoped<ICategoryRepositories, CategoryRepositories>();

builder.Services.AddScoped<IAccountRepositories, AccountRepositories>();
builder.Services.AddScoped<IEmployeeRepositories, EmployeeRepositories>();


builder.Services.AddScoped<IReportRepository, ReportRepository>();


// New Form Configuration repository
builder.Services.AddScoped<IFormConfigurationRepository, FormConfigurationRepository>();


// New Category Setup repository
builder.Services.AddScoped<ICategorySetupRepository, CategorySetupRepository>();

builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IAccessControlRepository, AccessControlRepository>();

builder.Services.AddScoped<IDropdownRepository, DropdownRepository>();
builder.Services.AddScoped<ICategoryChecklistRepository, CategoryChecklistRepository>();
builder.Services.AddScoped<IEmailRepository, EmailRepository>();
// Services
builder.Services.AddScoped<ICategoryService, CategoryService>();

// New Form Configuration service
builder.Services.AddScoped<IFormConfigurationService, FormConfigurationService>();

// New Category Setup service
builder.Services.AddScoped<ICategorySetupService, CategorySetupService>();

builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddScoped<ICategoryChecklistService, CategoryChecklistService>();
builder.Services.AddScoped<IDropdownService, DropdownService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddScoped<IEmployeeService, EmployeeService>();


// Email service
builder.Services.AddScoped<IEmailService, EmailService>();

// Cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(ConfigureCookieOptions);

builder.Services.AddAuthorization();

// QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();

// Common authentication paths
static void ConfigureCookieOptions(CookieAuthenticationOptions options)
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
}