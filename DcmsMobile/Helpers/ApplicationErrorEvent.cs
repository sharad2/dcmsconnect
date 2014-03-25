using System;
using System.Web.Management;

namespace DcmsMobile.Helpers
{
    public class ApplicationErrorEvent: WebErrorEvent
    {
        public ApplicationErrorEvent(string msg, object eventSource, Exception ex)
            : base(msg, eventSource, WebEventCodes.WebExtendedBase + 100, ex)
        {

        }
    }
}