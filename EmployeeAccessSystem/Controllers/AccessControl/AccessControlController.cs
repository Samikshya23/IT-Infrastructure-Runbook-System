using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace EmployeeAccessSystem.Controllers
{
    [Authorize]
    public class AccessControlController : Controller
    {
        private readonly IAccessControlService _service;
        private readonly IAccountService _accountService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccessControlController> _logger;

        public AccessControlController(
            IAccessControlService service,
            IAccountService accountService,
            IEmailService emailService,
            ILogger<AccessControlController> logger)
        {
            _service = service;
            _accountService = accountService;
            _emailService = emailService;
            _logger = logger;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            AccessControlDashboardModel model = await _service.GetDashboardAsync();

            return View(model);
        }

        // Manage users page
        public async Task<IActionResult> Users()
        {
            List<AccessControlUserModel> users = await _service.GetUsersAsync();

            return View(users);
        }

        // Add user modal
        public async Task<IActionResult> CreateUserModal()
        {
            RegisterModel model = new RegisterModel();

            List<AccessControlRoleModel> roles = await _service.GetRoleListAsync();

            ViewBag.Roles = new SelectList(roles, "RoleId", "RoleName");

            return PartialView("_CreateUserModal", model);
        }

        // Save new user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(RegisterModel model)
        {
            string plainPassword = model.Password;

            string message = await _accountService.RegisterAsync(model);

            // AccountService.RegisterAsync returns empty string when registration is successful
            if (string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    await _emailService.SendUserPasswordEmailAsync(
                        model.Email,
                        model.FullName,
                        plainPassword
                    );

                    TempData["Success"] = "User registered successfully and password email sent.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "User registered but password email sending failed. Email: {Email}", model.Email);

                    TempData["Success"] = "User registered successfully, but password email could not be sent.";
                }

                return RedirectToAction(nameof(Users));
            }

            TempData["Error"] = message;

            return RedirectToAction(nameof(Users));
        }

        // User permission list page
        public async Task<IActionResult> UserMenuAccess()
        {
            List<AccessControlUserModel> users = await _service.GetUsersAsync();

            return View(users);
        }

        // User details modal
        public async Task<IActionResult> DetailsModal(int accountId)
        {
            if (accountId <= 0)
            {
                return Content("error|Please select a valid user.");
            }

            AccessControlUserModel model = await _service.GetUserDetailAsync(accountId);

            if (model == null || model.AccountId <= 0)
            {
                return Content("error|User not found.");
            }

            return PartialView("_DetailsModal", model);
        }

        // User edit modal
        public async Task<IActionResult> EditUserModal(int accountId)
        {
            if (accountId <= 0)
            {
                return Content("error|Please select a valid user.");
            }

            AccessControlUserModel model = await _service.GetUserDetailAsync(accountId);

            if (model == null || model.AccountId <= 0)
            {
                return Content("error|User not found.");
            }

            List<AccessControlRoleModel> roles = await _service.GetRoleListAsync();

            ViewBag.Roles = new SelectList(roles, "RoleId", "RoleName", model.RoleId);

            return PartialView("_EditUserModal", model);
        }

        // Update user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(AccessControlUserModel model)
        {
            string message = await _service.UpdateUserAsync(model, GetCurrentUserId());

            SetMessage(message);

            return RedirectToAction(nameof(Users));
        }

        // Delete user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int accountId)
        {
            if (accountId <= 0)
            {
                TempData["Error"] = "Please select a valid user.";
                return RedirectToAction(nameof(Users));
            }

            string message = await _service.DeleteUserAsync(accountId, GetCurrentUserId());

            SetMessage(message);

            return RedirectToAction(nameof(Users));
        }

        // Manage roles page
        public async Task<IActionResult> Roles()
        {
            List<AccessControlRoleModel> roles = await _service.GetRolesAsync();

            return View(roles);
        }

        // Add role modal
        public IActionResult CreateRoleModal()
        {
            AccessControlRoleModel model = new AccessControlRoleModel
            {
                IsActive = true
            };

            return PartialView("_CreateRoleModal", model);
        }

        // Save role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(AccessControlRoleModel model)
        {
            string message = await _service.SaveRoleAsync(model.RoleName, GetCurrentUserId());

            SetMessage(message);

            return RedirectToAction(nameof(Roles));
        }

        // Edit role modal
        public async Task<IActionResult> EditRoleModal(int roleId)
        {
            if (roleId <= 0)
            {
                return Content("error|Please select a valid role.");
            }

            AccessControlRoleModel model = await _service.GetRoleByIdAsync(roleId);

            if (model == null || model.RoleId <= 0)
            {
                return Content("error|Role not found.");
            }

            return PartialView("_EditRoleModal", model);
        }

        // Update role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(AccessControlRoleModel model)
        {
            string message = await _service.UpdateRoleAsync(model, GetCurrentUserId());

            SetMessage(message);

            return RedirectToAction(nameof(Roles));
        }

        // Delete role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            if (roleId <= 0)
            {
                TempData["Error"] = "Please select a valid role.";
                return RedirectToAction(nameof(Roles));
            }

            string message = await _service.DeleteRoleAsync(roleId, GetCurrentUserId());

            SetMessage(message);

            return RedirectToAction(nameof(Roles));
        }

        // User permission modal
        public async Task<IActionResult> UserPermissionModal(int accountId)
        {
            if (accountId <= 0)
            {
                return Content("error|Please select a valid user.");
            }

            AccessControlUserModel user = await _service.GetUserDetailAsync(accountId);

            if (user == null || user.AccountId <= 0)
            {
                return Content("error|User not found.");
            }

            List<AccessControlRoleActionModel> permissions = await _service.GetUserPermissionItemsAsync(accountId);

            ViewBag.AccountId = user.AccountId;
            ViewBag.FullName = user.FullName;
            ViewBag.Email = user.Email;
            ViewBag.RoleName = user.RoleName;

            return PartialView("_UserPermissionModal", permissions);
        }

        // Old direct user permission page is not used now
        public IActionResult UserPermission(int accountId)
        {
            return RedirectToAction(nameof(UserMenuAccess));
        }

        // Save user permission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUserAccess(int accountId, List<int> checkedMenuIds)
        {
            if (checkedMenuIds == null)
            {
                checkedMenuIds = new List<int>();
            }

            List<int> uncheckedMenuIds = new List<int>();

            string message = await _service.SaveUserAccessAsync(accountId, checkedMenuIds, uncheckedMenuIds, GetCurrentUserId());

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (!string.IsNullOrWhiteSpace(message) && message.Contains("successfully"))
                {
                    return Json(new { success = true, message = message });
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    return Json(new { success = false, message = "Something went wrong. Please try again." });
                }

                return Json(new { success = false, message = message });
            }

            SetMessage(message);

            return RedirectToAction(nameof(UserMenuAccess));
        }

        // Clear user-specific permission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearUserAccess(int accountId)
        {
            string message = await _service.ClearUserAccessAsync(accountId, GetCurrentUserId());

            SetMessage(message);

            return RedirectToAction(nameof(UserMenuAccess));
        }

        /*
            Role permission is not used in the final flow.

            Final rule:
            Administrator has full access automatically.
            Normal user permission is assigned from Manage Users only.
            Roles are only user category.
        */

        public IActionResult RolePermissionModal(int roleId)
        {
            return Content("error|Role permission is not used. Please assign permission from Manage Users.");
        }

        public IActionResult RolePermission(int roleId)
        {
            TempData["Error"] = "Role permission is not used. Please assign permission from Manage Users.";

            return RedirectToAction(nameof(Roles));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveRoleAccess(int roleId, List<int> checkedMenuIds)
        {
            TempData["Error"] = "Role permission is not used. Please assign permission from Manage Users.";

            return RedirectToAction(nameof(Roles));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearRoleAccess(int roleId)
        {
            TempData["Error"] = "Role permission is not used. Please assign permission from Manage Users.";

            return RedirectToAction(nameof(Roles));
        }

        // Access denied page
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Logged-in account id
        private int GetCurrentUserId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userId, out int accountId))
            {
                return accountId;
            }

            return 0;
        }

        // TempData message for Toastr
        private void SetMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Something went wrong. Please try again.";
                return;
            }

            if (message.Contains("successfully"))
            {
                TempData["Success"] = message;
                return;
            }

            TempData["Error"] = message;
        }
    }
}