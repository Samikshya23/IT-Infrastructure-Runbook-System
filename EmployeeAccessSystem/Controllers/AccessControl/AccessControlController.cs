using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeAccessSystem.Controllers
{
    [Authorize]
 
    public class AccessControlController : Controller
    {
        private readonly IAccessControlService _service;

        public AccessControlController(IAccessControlService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            AccessControlDashboardModel dashboard = await _service.GetDashboardAsync();

            return View(dashboard);
        }

        public async Task<IActionResult> Users()
        {
            List<AccessControlUserModel> users = await _service.GetUsersAsync();

            return View(users);
        }

        public async Task<IActionResult> Roles()
        {
            List<AccessControlRoleModel> roles = await _service.GetRoleListAsync();

            return View(roles);
        }

        public async Task<IActionResult> UserMenuAccess()
        {
            List<AccessControlUserModel> users = await _service.GetUsersAsync();

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsModal(int accountId)
        {
            AccessControlUserModel user = await _service.GetUserDetailAsync(accountId);

            return PartialView("_DetailsModal", user);
        }

        [HttpGet]
        public async Task<IActionResult> EditUserModal(int accountId)
        {
            AccessControlUserModel user = await _service.GetUserDetailAsync(accountId);

            List<AccessControlRoleModel> roles = await _service.GetRolesAsync();

            ViewBag.Roles = new SelectList(roles, "RoleId", "RoleName", user.RoleId);

            return PartialView("_EditUserModal", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(AccessControlUserModel model)
        {
            string message = await _service.UpdateUserAsync(model);

            if (!string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = message;
                return RedirectToAction("Users");
            }

            TempData["Success"] = "Record updated successfully.";

            return RedirectToAction("Users");
        }

        [HttpGet]
        public async Task<IActionResult> AccessModal(int accountId)
        {
            AccessControlUserModel user = await _service.GetUserDetailAsync(accountId);

            List<AccessControlMenuModel> menus = await _service.GetMenusByUserAsync(accountId);

            ViewBag.SelectedUser = user;
            ViewBag.AccountId = accountId;

            return PartialView("_AccessModal", menus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAccess(int accountId, List<int> menuIds)
        {
            string message = await _service.SaveAccessAsync(accountId, menuIds);

            if (!string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = message;
                return RedirectToAction("UserMenuAccess");
            }

            TempData["Success"] = "Menu access updated successfully.";

            return RedirectToAction("UserMenuAccess");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAccess(int accountId)
        {
            string message = await _service.RemoveAccessAsync(accountId);

            if (!string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = message;
                return RedirectToAction("UserMenuAccess");
            }

            TempData["Success"] = "Menu access removed successfully.";

            return RedirectToAction("UserMenuAccess");
        }

        [HttpGet]
        public IActionResult CreateRoleModal()
        {
            return PartialView("_CreateRoleModal", new AccessControlRoleModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(AccessControlRoleModel model)
        {
            string message = await _service.SaveRoleAsync(model.RoleName);

            if (!string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = message;
                return RedirectToAction("Roles");
            }

            TempData["Success"] = "Record saved successfully.";

            return RedirectToAction("Roles");
        }

        [HttpGet]
        public async Task<IActionResult> EditRoleModal(int roleId)
        {
            AccessControlRoleModel role = await _service.GetRoleByIdAsync(roleId);

            return PartialView("_EditRoleModal", role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(AccessControlRoleModel model)
        {
            string message = await _service.UpdateRoleAsync(model);

            if (!string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = message;
                return RedirectToAction("Roles");
            }

            TempData["Success"] = "Record updated successfully.";

            return RedirectToAction("Roles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            string message = await _service.DeleteRoleAsync(roleId);

            if (!string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = message;
                return RedirectToAction("Roles");
            }

            TempData["Success"] = "Record deleted successfully.";

            return RedirectToAction("Roles");
        }
    }
}