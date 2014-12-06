using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using System.Web.SessionState;
using EclipseLibrary.Mvc.Html;
using System.Net;

// This namespace should be EclipseLibrary.Mvc.Controllers. It is not being changed to maintain compatibility
namespace EclipseLibrary.Mvc.Controllers
{
    /// <summary>
    /// Provides the framewark for persisting status messages and model errors across redirects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You andd errors to ModelState as you always did. Additionally, you can use the <see cref="AddStatusMessage"/> to add status messages
    /// meant for the user. Make sure each view calls the <see cref="StatusSummaryExtensions.StatusSummary"/> helper which is status message aware.
    /// </para>
    /// <para>
    /// The controller is decorated with the SessionState Required attribute to ensure that TempData is accessible.
    /// </para>
    /// <para>
    /// Sharad 23 Jun 2011: Added RenderPartialViewToString. Removed it on 5 Aug 2011.
    /// </para>
    /// <para>
    /// Sharad 7 Jul 2011: Explicitly setting the culture to US culture
    /// </para>
    /// <para>
    /// Sharad 12 Jul 2011: Added method TryValidateList()
    /// </para>
    /// <para>
    /// Sharad 14 Jul 2011: Added utility class AutoCompleteItem
    /// </para>
    /// <para>
    /// Sharad 1 Ocr 2011: Added ValidationErrorResult
    /// </para>
    /// </remarks>
    [SessionState(SessionStateBehavior.Required)]
    public abstract partial class EclipseController : Controller
    {
        /// <summary>
        /// Only if we are redirecting, transfer model errors to temp data
        /// </summary>
        /// <param name="filterContext"></param>
        /// <remarks>
        /// We do this just before the result executes. This gives ample time to derived controllers to manipulate the model state.
        /// </remarks>
        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.Result is RedirectToRouteResult && !ModelState.IsValid)
            {
                // Since we are redirecting, ensure that Modelstate errors are transferred to TempData
                var errors = ModelState.SelectMany(p => p.Value.Errors);
                StatusSummaryExtensions.GetErrorList(TempData).AddRange(errors.Select(p => p.Exception == null ? p.ErrorMessage : p.Exception.Message));
            }
            base.OnResultExecuting(filterContext);
        }

        /// <summary>
        /// Add a status message which can then be displayed appropriately by <see cref="StatusSummaryExtensions.StatusSummary"/>
        /// </summary>
        /// <param name="msg"></param>
        /// <remarks>
        /// public so that attributes like <see cref="AuthorizeExAttribute"/> can access it.
        /// </remarks>
        public void AddStatusMessage(string msg)
        {
            StatusSummaryExtensions.GetStatusList(TempData).Add(msg);
        }

        /// <summary>
        /// Returns the added status messages
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Added by Sharad on 23 May 2012. Allows apps, e.g. Carton Manager, to access the status messages and return them as an ajax list
        /// </remarks>
        protected IEnumerable<string> GetStatusMessages()
        {
            return StatusSummaryExtensions.GetStatusList(TempData);
        }

        /// <summary>
        /// This returns validationsummary text as content with response status of 203.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// The text returned is the same as what Html.ValidationSummary() would have returned in the view. Thuis function
        /// is designed for Ajax sceneraios where you can return validation errors as string and have your script replace the existing validation summary.
        /// </para>
        /// </remarks>
        protected virtual ContentResult ValidationErrorResult()
        {
            var div = new TagBuilder("div");
            div.MergeAttribute("data-valmsg-summary", "true");
            if (ModelState.IsValid)
            {
                div.MergeAttribute("class", "validation-summary-valid");
                div.InnerHtml = "<li style=\"display:none\"></li>";
            }
            else
            {
                div.MergeAttribute("class", "validation-summary-errors");
                var sb = new StringBuilder();
                sb.Append("<ul>");
                foreach (var error in ModelState.Values.SelectMany(p => p.Errors))
                {
                    sb.AppendFormat("<li>{0}</li>", error.ErrorMessage);
                }
                sb.Append("</ul>");
                div.InnerHtml = sb.ToString();
            }
            this.HttpContext.Response.StatusCode = 203;
            return Content(div.ToString());
        }

        /// <summary>
        /// For now, only US culture is supported
        /// </summary>
        protected override void ExecuteCore()
        {
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture =
                CultureInfo.CreateSpecificCulture("en-US");
            base.ExecuteCore();
        }

        /// <summary>
        /// Sharad 20 Oct 2012: For AJAX requests, the yellow screen is unreadable. This attribute returns exception content instead of the HTML content of the yellow screen.
        /// Also raises error event
        /// </summary>
        /// <remarks>
        /// <code>
        /// <![CDATA[
        /// $.ajax({
        /// 
        ///   error: function(
        /// 
        /// });
        /// 
        /// 
        /// 
        /// ]]>
        /// </code>
        /// </remarks>
        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest() && !filterContext.ExceptionHandled)
            {
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                filterContext.Result = new ContentResult { Content = filterContext.Exception.Message };
                filterContext.ExceptionHandled = true;
                // Sharad 24 Mar 2014: Logging the exception
                this.HttpContext.Trace.Warn("AjaxException", "Exception encountered in AJAX request", filterContext.Exception);
            }
            else
            {
                // Default handling
                base.OnException(filterContext);
            }
            if (filterContext.ExceptionHandled)
            {
                // Log exceptions which occurred but were internally handled, such as our AJAX request exception
                var ev = new MvcErrorEvent("EclipseController.OnException log of handled exception", this, filterContext.Exception);
                ev.Raise();
            }
        }
    }
}
