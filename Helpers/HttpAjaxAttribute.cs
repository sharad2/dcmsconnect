using System;
using System.Reflection;
using System.Web.Mvc;

namespace DcmsMobile.Receiving.Helpers
{
    /// <summary>
    /// Action selector which returns true only for AJAX requests
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class HttpAjaxAttribute : ActionMethodSelectorAttribute
    {
        private bool _ajaxRequest;
        public HttpAjaxAttribute(bool ajaxRequest)
        {
            _ajaxRequest = ajaxRequest;
        }
        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            return controllerContext.HttpContext.Request.IsAjaxRequest() == _ajaxRequest;
        }
    }
}

