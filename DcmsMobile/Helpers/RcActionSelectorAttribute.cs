using System;
using System.Configuration;
using System.Reflection;
using System.Web.Mvc;

namespace DcmsMobile.Helpers
{
    [Obsolete]
    public class RcActionSelectorAttribute:ActionMethodSelectorAttribute
    {
        ///// <summary>
        ///// Selects the action based on whether this is an RC website
        ///// </summary>
        ///// <param name="rcAction"></param>
        //public RcActionSelectorAttribute()
        //{
        //    if (__urlRc == null)
        //    {
        //        __urlRc = (ConfigurationManager.AppSettings["RcUrl"] ?? string.Empty).TrimEnd(' ', '/').Trim();
        //    }
        //}

        public bool IsRcAction { get; set; }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            bool isRc;
            var urlRc = UrlRc;
            if (string.IsNullOrEmpty(urlRc))
            {
                // RC feature is not being used
                isRc = false;
            }
            else
            {
                var appUrlBase = controllerContext.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + controllerContext.HttpContext.Request.ApplicationPath;
                // If the root of our app is the same as the rc url, then we are RC
                isRc = string.Compare(urlRc, appUrlBase.TrimEnd('/'), true) == 0;
            }

            return isRc == IsRcAction;
        }

        private static object __urlRc;
        public static string UrlRc
        {
            get
            {
                if (__urlRc == null)
                {
                    __urlRc = (ConfigurationManager.AppSettings["RcUrl"] ?? string.Empty).TrimEnd(' ', '/').Trim();
                }
                return (string)__urlRc;
            }
        }
    }
}