using System;
using System.Web.Mvc;

namespace EclipseLibrary.Mvc.Controllers
{
    /// <summary>
    /// Should be applied to actions which are called via ajax. Returns status code 203 with the exception message as content.
    /// </summary>
    /// <remarks>
    /// As a safety precaution, it does not do anything if the request is not an Ajax request
    /// </remarks>
    [Obsolete("No longer necessary. Automatically handled by EclipseController. Script should look for status 500 instead of status 203.")]
    public class HandleAjaxErrorAttribute : HandleErrorAttribute
    {
        private readonly bool _jsonResponse;

        /// <summary>
        /// The reponse can be sent as string or json
        /// </summary>
        /// <param name="jsonResponse">Whether the reponse is needed as JSON, or as a string.</param>
        public HandleAjaxErrorAttribute(bool jsonResponse = false)
        {
            _jsonResponse = jsonResponse;
        }

        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.HttpContext.Response.StatusCode = 203;
                if (_jsonResponse)
                {
                    filterContext.Result = new JsonResult { Data = filterContext.Exception.Message, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                else
                {
                    filterContext.Result = new ContentResult { Content = filterContext.Exception.Message };
                }
                filterContext.ExceptionHandled = true;
            }
            else
            {
                base.OnException(filterContext);
            }
        }
    }
}