using System;
using System.Web.Management;

namespace EclipseLibrary.Mvc.Controllers
{
    /// <summary>
    /// Use this to log errors to the health monitoring system
    /// </summary>
    internal class MvcErrorEvent: WebErrorEvent
    {
        public MvcErrorEvent(string msg, object eventSource, Exception ex)
            : base(msg, eventSource, WebEventCodes.WebExtendedBase + 101, ex)
        {

        }
    }
}