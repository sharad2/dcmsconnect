using System.Web.Mvc;

namespace DcmsMobile.Helpers
{
    public class FeatureFolderViewEngine : RazorViewEngine
    {
        public FeatureFolderViewEngine()
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

            locs = new[]
                                    {
                                        "~/DcmsMobile/{1}/{0}.cshtml",
                                        "~/DcmsMobile/SharedViews/{0}.cshtml", // Replacement for "Views/Shared"
                                    };

            ViewLocationFormats =
                MasterLocationFormats =
                PartialViewLocationFormats = locs;

            FileExtensions = new[] { "cshtml" };
        }
    }
}