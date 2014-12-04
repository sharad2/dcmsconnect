using System.Linq;
using System.Web.Mvc;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.BoxPick.Helpers
{
    /// <summary>
    /// If any field other than posted fields is invalid, redirect to home page.
    /// </summary>
    /// <remarks>
    /// For this attribute to work, you must accept your model as one of the action parameters.
    /// </remarks>
    public class BoxPickContextCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.Controller.ViewData.ModelState.IsValid)
            {
                var unpostedInvalidField = filterContext.Controller.ViewData.ModelState
                    .Where(p => p.Value.Errors.Count > 0 && !filterContext.Controller.ValueProvider.ContainsPrefix(p.Key));
                if (unpostedInvalidField.Any())
                {
                    var actionName = filterContext.ActionDescriptor.ActionName;
                    var controllerName = string.Format("{0}", filterContext.RouteData.Values["controller"]);
                    // We do not want to interfere with GET requests to home page
                    if (filterContext.HttpContext.Request.RequestType != "GET" || actionName != "Index" || controllerName != "Home")
                    {
                        // Avoid infinite loops. Do not redirect to the url which is being requested
                        var result = new RedirectResult(UrlHelper.GenerateContentUrl("~/BoxPick", filterContext.HttpContext));
                        filterContext.Controller.ViewData.ModelState.AddModelError("", "Your session may have expired. Please start over.");
                        filterContext.Result = result;
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
