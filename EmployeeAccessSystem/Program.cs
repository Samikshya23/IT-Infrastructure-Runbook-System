using EmployeeAccessSystem.Filters;
using EmployeeAccessSystem.Repositories;
using EmployeeAccessSystem.Services;
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
builder.Services.AddScoped<ISubCategoryRepositories, SubCategoryRepositories>();
builder.Services.AddScoped<IAccountRepositories, AccountRepositories>();
builder.Services.AddScoped<IEmployeeRepositories, EmployeeRepositories>();
builder.Services.AddScoped<IDepartmentRepositories, DepartmentRepositories>();
builder.Services.AddScoped<IProductSetupRepositories, ProductSetupRepositories>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IProductConfigurationRepository, ProductConfigurationRepository>();
builder.Services.AddScoped<IProductSetupConfigurationRepository, ProductSetupConfigurationRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IAccessControlRepository, AccessControlRepository>();
builder.Services.AddScoped<IProductEntryRepository, ProductEntryRepository>();
builder.Services.AddScoped<IDropdownRepository, DropdownRepository>();

// Services
builder.Services.AddScoped<IProductConfigurationService, ProductConfigurationService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddScoped<IProductEntryService, ProductEntryService>();
builder.Services.AddScoped<IDropdownService, DropdownService>();
builder.Services.AddScoped<IProductSetupConfigurationService, ProductSetupConfigurationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IProductSetupService, ProductSetupService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();

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