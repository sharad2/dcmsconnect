using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using EclipseLibrary.Mvc.Controllers;

namespace EclipseLibrary.Mvc
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// <para>
    /// Originally Inspired by http://www.hanselman.com/blog/ABetterASPNETMVCMobileDeviceCapabilitiesViewEngine.aspx.
    /// Sep 17 2011: Code changed to mitigate Release Mode 
    /// bug as described in http://www.hanselman.com/blog/NuGetPackageOfTheWeek10NewMobileViewEnginesForASPNETMVC3SpeccompatibleWithASPNETMVC4.aspx
    /// </para>
    /// <para>
    /// Sharad 4 Nov 2011: Using a cookie to force mobile emulation on desktop.
    /// </para>
    /// </remarks>
    [Obsolete("Not needed in MVC 4")]
    public class MobileCapableRazorViewEngine : RazorViewEngine
    {


        public MobileCapableRazorViewEngine() 
        {
            // Only look for cshtml extensions. Don't worry about vbhtml extension.
            this.FileExtensions = new[] {"cshtml"};
            this.ViewLocationFormats = this.ViewLocationFormats.Where(p => p.EndsWith("cshtml")).ToArray();
            this.AreaViewLocationFormats = this.AreaViewLocationFormats.Where(p => p.EndsWith("cshtml")).ToArray();
            this.MasterLocationFormats = this.MasterLocationFormats.Where(p => p.EndsWith("cshtml")).ToArray();
            this.AreaMasterLocationFormats = this.AreaMasterLocationFormats.Where(p => p.EndsWith("cshtml")).ToArray();
        }

        ///// <summary>
        ///// Alwas returns false for actual Mobile Devices. For non mobile devices, returns whether a cookie is forcing emulation of a mobile device.
        ///// </summary>
        ///// <param name="controllerContext"></param>
        ///// <returns></returns>
        //public bool IsEmulatingMobileDevice(ControllerContext controllerContext)
        //{
        //    return !controllerContext.HttpContext.Request.Browser.IsMobileDevice &&
        //        controllerContext.HttpContext.Request.Cookies.AllKeys.Any(p => p == COOKIE_EMULATEMOBILE);
        //}


        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return DoFindView(controllerContext, viewName, masterName, useCache, false);
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return DoFindView(controllerContext, partialViewName, null, useCache, true);
        }

        private ViewEngineResult DoFindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache, bool isPartial)
        {
            // Ringscanner User agent: Mozilla/4.0 (compatible; MSIE 6.0; Windows CE)
            var overrideViewName = controllerContext.HttpContext.Request.Browser.IsMobileDevice || MobileEmulation.IsEmulatingMobileDevice(controllerContext) ? viewName + ".Mobile" : viewName;
            var result = NewFindView(controllerContext, overrideViewName, masterName, useCache, isPartial);

            // If we're looking for a Mobile view and couldn't find it try again without modifying the viewname
            if (overrideViewName.Contains(".Mobile") && (result == null || result.View == null))
            {
                result = NewFindView(controllerContext, viewName, masterName, useCache, isPartial);
            }
            return result;
        }


        private ViewEngineResult NewFindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache, bool isPartial)
        {
            // Get the name of the controller from the path
            var controller = controllerContext.RouteData.Values["controller"].ToString();
            object token;
            var b = controllerContext.RouteData.DataTokens.TryGetValue("area", out token);
            var area = b ? string.Format("{0}", token) : "";

            // Create the key for caching purposes
            var keyPath = Path.Combine(area, controller, viewName);

            // Try the cache
            // Caching only occurs when debug=false in web.config.
            // See http://stackoverflow.com/questions/2127030/asp-net-mvc-viewengine-viewlocationcache-getviewlocation-returns-null
            if (useCache)
            {
                //If using the cache, check to see if the location is cached.
                var cacheLocation = ViewLocationCache.GetViewLocation(controllerContext.HttpContext, keyPath);
                if (!string.IsNullOrWhiteSpace(cacheLocation))
                {
                    return new ViewEngineResult(isPartial ? CreatePartialView(controllerContext, cacheLocation) : CreateView(controllerContext, cacheLocation, masterName), this);
                }
            }

            // Remember the attempted paths, if not found display the attempted paths in the error message.
            var attempts = new List<string>();

            var locationFormats = string.IsNullOrEmpty(area) ? ViewLocationFormats : AreaViewLocationFormats;

            // for each of the paths defined, format the string and see if that path exists. When found, cache it.
            foreach (var currentPath in locationFormats.Select(rootPath => string.IsNullOrEmpty(area) ? string.Format(rootPath, viewName, controller)
                                                                               : string.Format(rootPath, viewName, controller, area)))
            {
                if (FileExists(controllerContext, currentPath))
                {
                    ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, keyPath, currentPath);

                    return new ViewEngineResult(isPartial ? CreatePartialView(controllerContext, currentPath) : CreateView(controllerContext, currentPath, masterName), this);
                }

                // If not found, add to the list of attempts.
                attempts.Add(currentPath);
            }

            // if not found by now, simply return the attempted paths.
            return new ViewEngineResult(attempts.Distinct().ToList());
        }
    }
}
