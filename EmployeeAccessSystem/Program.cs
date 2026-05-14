using EmployeeAccessSystem.Repositories;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using QuestPDF.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ICoreDbConnection, CoreDbConnection>();
builder.Services.AddScoped<ICategoryRepositories, CategoryRepositories>();
builder.Services.AddScoped<ISubCategoryRepositories, SubCategoryRepositories>();
builder.Services.AddScoped<IAccountRepositories, AccountRepositories>();
builder.Services.AddScoped<IEmployeeRepositories, EmployeeRepositories>();
builder.Services.AddScoped<IDepartmentRepositories, DepartmentRepositories>();
builder.Services.AddScoped<IAccessRequestRepositories, AccessRequestRepositories>();
builder.Services.AddScoped<ISupervisorRepositories, SupervisorRepositories>();
builder.Services.AddScoped<IProductSetupRepositories, ProductSetupRepositories>();
builder.Services.AddScoped<IReportRepository, ReportRepository>(); 
builder.Services.AddScoped<IProductConfigurationRepository, ProductConfigurationRepository>();
builder.Services.AddScoped<IProductConfigurationService, ProductConfigurationService>();
builder.Services.AddScoped<IProductSetupConfigurationRepository, ProductSetupConfigurationRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IProductEntryRepository, ProductEntryRepository>();
builder.Services.AddScoped<IProductEntryService, ProductEntryService>();
builder.Services.AddScoped<IDropdownRepository, DropdownRepository>();
builder.Services.AddScoped<IDropdownService, DropdownService>();

builder.Services.AddScoped<IProductSetupConfigurationService, ProductSetupConfigurationService>();

builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<IProductSetupService, ProductSetupService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();
builder.Services.AddScoped<IAccessRequestService, AccessRequestService>();
builder.Services.AddScoped<ISupervisorService, SupervisorService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

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
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();