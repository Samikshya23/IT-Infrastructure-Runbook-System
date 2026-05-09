using Microsoft.AspNetCore.Mvc;

namespace EmployeeAccessSystem.Components.DeleteConfirmation
{
    public class DeleteConfirmationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(
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

            return View();
        }
    }
}