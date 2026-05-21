using System.Security.Claims;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAccessSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAccountService accountService,
            ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        // Login page
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            _logger.LogInformation("Login page opened.");

            return View();
        }

        // Login submit
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            _logger.LogInformation("Login submitted. Email: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login validation failed. Email: {Email}", model.Email);

                return View(model);
            }

            string error = await _accountService.LoginAsync(model);

            if (!string.IsNullOrWhiteSpace(error))
            {
                _logger.LogWarning("Login failed. Email: {Email}, Error: {Error}", model.Email, error);

                ViewBag.Error = error;

                return View(model);
            }

            Account? account = await _accountService.GetAccountByEmailAsync(model.Email);

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

            // These claims are important for menu and permission checking
            claims.Add(new Claim("RoleId", account.RoleId.ToString()));
            claims.Add(new Claim("HasFullAccess", account.HasFullAccess.ToString()));

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            AuthenticationProperties authProperties = new AuthenticationProperties();
            authProperties.IsPersistent = true;
            authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties
            );

            _logger.LogInformation(
                "Login successful. AccountId: {AccountId}, Email: {Email}, Role: {Role}, HasFullAccess: {HasFullAccess}",
                account.AccountId,
                account.Email,
                account.RoleName,
                account.HasFullAccess
            );

            // Full access admin goes to Access Control dashboard
            if (account.HasFullAccess == true)
            {
                return RedirectToAction("Index", "AccessControl");
            }

            // Normal user goes to first allowed system page
            return RedirectToAction("Index", "ProductEntry");
        }

        // Logout submit
        // Navbar logout form uses POST, so this action must also be POST
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            string email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

            _logger.LogInformation("Logout clicked. Email: {Email}", email);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }

        // Common access denied page
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}