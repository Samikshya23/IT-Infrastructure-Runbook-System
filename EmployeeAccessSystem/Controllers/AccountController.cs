using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;
using EmployeeAccessSystem.Services;

namespace EmployeeAccessSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IDepartmentRepositories _departmentRepo;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAccountService accountService,
            IDepartmentRepositories departmentRepo,
            ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _departmentRepo = departmentRepo;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            _logger.LogInformation("Register page opened.");

            var departments = await _departmentRepo.GetAllAsync();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            _logger.LogInformation("Register submitted. Email: {Email}", model.Email);

            var departments = await _departmentRepo.GetAllAsync();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName", model.DepartmentId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Register validation failed. Email: {Email}", model.Email);
                return View(model);
            }

            string error = await _accountService.RegisterAsync(model);

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Register failed. Email: {Email}, Error: {Error}", model.Email, error);
                ViewBag.Error = error;
                return View(model);
            }

            _logger.LogInformation("Registration successful. Email: {Email}", model.Email);

            TempData["Success"] = "Registration successful. Please login.";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            _logger.LogInformation("Login page opened.");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            _logger.LogInformation("Login submitted. Email: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login validation failed. Email: {Email}", model.Email);
                return View(model);
            }

            string error = await _accountService.LoginAsync(model);

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Login failed. Email: {Email}, Error: {Error}", model.Email, error);
                ViewBag.Error = error;
                return View(model);
            }

            Account account = await _accountService.GetAccountByEmailAsync(model.Email);

            if (account == null)
            {
                _logger.LogWarning("Account not found after successful login. Email: {Email}", model.Email);
                ViewBag.Error = "Account not found.";
                return View(model);
            }

            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, account.FullName ?? ""));
            claims.Add(new Claim(ClaimTypes.Email, account.Email ?? ""));
            claims.Add(new Claim(ClaimTypes.Role, account.RoleName ?? ""));

            ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            ClaimsPrincipal claimsPrincipal =
                new ClaimsPrincipal(claimsIdentity);

            AuthenticationProperties authProperties =
                new AuthenticationProperties();

            authProperties.IsPersistent = true;

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties);

            _logger.LogInformation(
                "Login successful. AccountId: {AccountId}, Email: {Email}, Role: {Role}",
                account.AccountId,
                account.Email,
                account.RoleName);

            string roleName = "";

            if (account.RoleName != null)
            {
                roleName = account.RoleName.Trim();
            }

            if (roleName.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Employee");
            }
            else if (roleName.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Employee");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Logout()
        {
            string email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

            _logger.LogInformation("Logout clicked. Email: {Email}", email);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}