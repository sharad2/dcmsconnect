using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.Helpers
{
    [Flags]
    public enum RazorViewEngineFolders
    {
        AreaFeatureFolders = 0x1,
        ProjectFeatureFolders = 0x2,
        AreaConventionalFolders = 0x4
    }
    /// <summary>
    /// Inspired by T4MVC feature folder documentation https://t4mvc.codeplex.com/documentation
    /// </summary>
    /// <remarks>
    /// This view engine differs from the MVC RazorViewEngine in the following ways:
    ///  1. We never look for area views with the DcmsMobile Views folder. This ensures that DCMSMobile views do not get accidentally in place of missing area views.
    ///  2. It allows the areas to be organized using the feature folder convention and/or regular MVC convention.
    ///  3. It looks for cshtml extensions only.
    /// </remarks>
    internal class FeatureFolderViewEngine : RazorViewEngine
    {
        /// <summary>
        /// This constructor expects each area to be organized using feature folder convention.
        /// It clears out all view locations so that views of the main project do not serve as a default for area views
        /// </summary>
        /// <param name="eFolders">You can pass multiple flags if some of your areas are organized conventionally and some are organized using feature format</param>
        public FeatureFolderViewEngine(RazorViewEngineFolders eFolders)
        {
            // {0} ActionName; {1} ControllerName; {2} AreaName

            var list = new List<string>();
            if (eFolders.HasFlag(RazorViewEngineFolders.AreaConventionalFolders))
            {
                // If conventional support is needed, add the current area folders
                list.AddRange(this.AreaViewLocationFormats.Where(p => p.EndsWith(".cshtml")));
            }
            if (eFolders.HasFlag(RazorViewEngineFolders.AreaFeatureFolders))
            {
                var locs = new[]
                                    {
                                        "~/Areas/{2}/{1}/{0}.cshtml",
                                        "~/Areas/{2}/SharedViews/{0}.cshtml", // Replacement for "Views/Shared"
                                    };
                list.AddRange(locs);
            }


            AreaViewLocationFormats =
                AreaMasterLocationFormats =
                AreaPartialViewLocationFormats = list.ToArray();

            // Does not look for views within the main project.
            ViewLocationFormats =
                MasterLocationFormats =
                PartialViewLocationFormats = new string[0];
            FileExtensions = new[] { "cshtml" };
        }

        /// <summary>
        /// Allows organization of MVC files based on feature as described in T4MVC docs.
        /// If you have some areas organized in the conventional way, then you must use the default RazorViewEngine also.
        /// </summary>
        /// <param name="mainFeatureFolder">Name of the top level folder in which the views of the main application are organized.
        /// If not specified, then top level views are not supported. You must use the default RazorViewEngine to access top level views.
        /// </param>
        public FeatureFolderViewEngine(string mainFeatureFolder)
        {
            if (string.IsNullOrEmpty(mainFeatureFolder))
            {
                throw new ArgumentNullException("mainFeatureFolder");
            }

            // Does not look for files within areas
            AreaViewLocationFormats =
                AreaMasterLocationFormats =
                AreaPartialViewLocationFormats = new string[0];

            var locs = new[]
                {
                    string.Format("~/{0}/{{1}}/{{0}}.cshtml", mainFeatureFolder),
                    string.Format("~/{0}/SharedViews/{{0}}.cshtml", mainFeatureFolder), // Replacement for "Views/Shared"
                };

            ViewLocationFormats =
                MasterLocationFormats =
                PartialViewLocationFormats = locs;


            FileExtensions = new[] { "cshtml" };
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            var hasArea = controllerContext.RouteData.DataTokens.ContainsKey("area");
            if ((hasArea && AreaViewLocationFormats.Length == 0) || (!hasArea && ViewLocationFormats.Length == 0))
            {
                // Prevent the base class complains that ViewLocationFormats cannot be null or empty.
                return new ViewEngineResult(Enumerable.Empty<string>());
            }
            var x =  base.FindView(controllerContext, viewName, masterName, useCache);
            return x;
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            var hasArea = controllerContext.RouteData.DataTokens.ContainsKey("area");
            if ((hasArea && AreaPartialViewLocationFormats.Length == 0) || (!hasArea && PartialViewLocationFormats.Length == 0))
            {
                // Prevent the base class complains that PartialViewLocationFormats cannot be null or empty.
                return new ViewEngineResult(Enumerable.Empty<string>());
            }
            return base.FindPartialView(controllerContext, partialViewName, useCache);
        }

    }
}