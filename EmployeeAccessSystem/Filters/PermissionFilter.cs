using System;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using EmployeeAccessSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EmployeeAccessSystem.Filters
{
    public class PermissionFilter : IAsyncActionFilter
    {
        private readonly string _connectionString;

        public PermissionFilter(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Main permission check before every controller action
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                string controllerName = Convert.ToString(context.RouteData.Values["controller"]);
                string actionName = Convert.ToString(context.RouteData.Values["action"]);

                // Allow public actions
                if (IsPublicAction(controllerName, actionName))
                {
                    await next();
                    return;
                }

                // Allow actions with [AllowAnonymous]
                if (HasAllowAnonymous(context))
                {
                    await next();
                    return;
                }

                // Check login
                if (context.HttpContext.User == null ||
                    context.HttpContext.User.Identity == null ||
                    !context.HttpContext.User.Identity.IsAuthenticated)
                {
                    SetDeniedResult(context, "Please login first.", 401, true);
                    return;
                }

                int accountId = GetAccountId(context);

                if (accountId <= 0)
                {
                    SetDeniedResult(context, "Access denied. Invalid user account.", 403, false);
                    return;
                }

                bool hasPermission = await CheckPermissionAsync(accountId, controllerName, actionName);

                if (!hasPermission)
                {
                    SetDeniedResult(context, "Access denied. You do not have permission.", 403, false);
                    return;
                }

                await next();
            }
            catch
            {
                SetDeniedResult(context, "Access denied. Permission check failed.", 403, false);
            }
        }

        // Call sp_Menu_Manage CHECKPERMISSION
        private async Task<bool> CheckPermissionAsync(int accountId, string controllerName, string actionName)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "CHECKPERMISSION");
                parameters.Add("AccountId", accountId);
                parameters.Add("ControllerName", controllerName);
                parameters.Add("ActionName", actionName);

                AccessCheckModel result = await conn.QueryFirstOrDefaultAsync<AccessCheckModel>(
                    "dbo.sp_Menu_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                {
                    return false;
                }

                return result.HasPermission;
            }
            catch
            {
                return false;
            }
        }

        // Get AccountId from login claim
        private int GetAccountId(ActionExecutingContext context)
        {
            string accountIdText = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(accountIdText))
            {
                accountIdText = context.HttpContext.User.FindFirst("AccountId")?.Value;
            }

            int accountId = 0;
            int.TryParse(accountIdText, out accountId);

            return accountId;
        }

        // Public pages
        private bool IsPublicAction(string controllerName, string actionName)
        {
            if (string.IsNullOrWhiteSpace(controllerName) || string.IsNullOrWhiteSpace(actionName))
            {
                return true;
            }

            if (controllerName == "Account" && actionName == "Login")
            {
                return true;
            }

            if (controllerName == "Account" && actionName == "Logout")
            {
                return true;
            }

            if (controllerName == "Account" && actionName == "AccessDenied")
            {
                return true;
            }

            if (controllerName == "Home" && actionName == "Index")
            {
                return true;
            }

            return false;
        }

        // Check AllowAnonymous attribute
        private bool HasAllowAnonymous(ActionExecutingContext context)
        {
            foreach (object metadata in context.ActionDescriptor.EndpointMetadata)
            {
                if (metadata is AllowAnonymousAttribute)
                {
                    return true;
                }
            }

            return false;
        }

        // AJAX gets JSON, normal request goes to page
        private void SetDeniedResult(ActionExecutingContext context, string message, int statusCode, bool isLoginRequired)
        {
            if (IsAjaxRequest(context))
            {
                context.Result = new JsonResult(new
                {
                    success = false,
                    message = message
                })
                {
                    StatusCode = statusCode
                };

                return;
            }

            if (isLoginRequired)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }

        // AJAX check
        private bool IsAjaxRequest(ActionExecutingContext context)
        {
            string requestedWith = context.HttpContext.Request.Headers["X-Requested-With"];

            if (requestedWith == "XMLHttpRequest")
            {
                return true;
            }

            return false;
        }
    }
}