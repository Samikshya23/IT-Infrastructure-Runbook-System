using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EmployeeAccessSystem.Components.DeleteConfirmation
{
    public class DeleteConfirmationViewComponent : ViewComponent
    {
        // Load reusable delete confirmation modal
        public Task<IViewComponentResult> InvokeAsync(
            string formId,
            string actionName,
            string controllerName,
            string hiddenInputName,
            int hiddenInputValue,
            string message,
            string itemName)
        {
            ViewBag.FormId = formId;
            ViewBag.ActionName = actionName;
            ViewBag.ControllerName = controllerName;
            ViewBag.HiddenInputName = hiddenInputName;
            ViewBag.HiddenInputValue = hiddenInputValue;
            ViewBag.Message = message;
            ViewBag.ItemName = itemName;

            IViewComponentResult result =
                View("~/Components/DeleteConfirmation/Default.cshtml");

            return Task.FromResult(result);
        }
    }
}