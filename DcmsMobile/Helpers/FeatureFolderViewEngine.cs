using System.Web.Mvc;

namespace DcmsMobile.Helpers
{
    /// <summary>
    /// Inspired by T4MVC documentation https://t4mvc.codeplex.com/documentation
    /// </summary>
    internal class FeatureFolderViewEngine : RazorViewEngine
    {
        /// <summary>
        /// Allows organization of MVC files based on feature as described in T4MVC docs.
        /// If you have some areas organized in the conventional way, then you must use the default RazorViewEngine also.
        /// </summary>
        /// <param name="mainFeatureFolder">Name of the top level folder in which the views of the main application are organized.
        /// If not specified, then top level views are not supported. You must use the default RazorViewEngine to access top level views.
        /// </param>
        public FeatureFolderViewEngine(string mainFeatureFolder)
        {

            // {0} ActionName; {1} ControllerName; {2} AreaName
            var locs = new[]
                                    {
                                        "~/Areas/{2}/{1}/{0}.cshtml",
                                        "~/Areas/{2}/SharedViews/{0}.cshtml", // Replacement for "Views/Shared"
                                    };
            AreaViewLocationFormats =
                AreaMasterLocationFormats =
                AreaPartialViewLocationFormats = locs;

            if (string.IsNullOrEmpty(mainFeatureFolder))
            {
                locs = new string[0];   // Empty
            }
            {
                locs = new[]
                                    {
                                        string.Format("~/{0}/{{1}}/{{0}}.cshtml", mainFeatureFolder),
                                        string.Format("~/{0}/SharedViews/{{0}}.cshtml", mainFeatureFolder), // Replacement for "Views/Shared"
                                    };
            }

            ViewLocationFormats =
                MasterLocationFormats =
                PartialViewLocationFormats = locs;


            FileExtensions = new[] { "cshtml" };
        }
    }
}