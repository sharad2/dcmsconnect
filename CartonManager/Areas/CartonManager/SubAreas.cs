using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.CartonManager.Areas.CartonManager
{
    public class SubAreas
    {
        // <summary>
        // Enables DcmsMobile to check whether we are prepared to respond to the search text entered by the user
        // </summary>
        // <remarks>
        // <para>
        // Attribute <c>[MetadataType(typeof(SubAreas))]</c> must be applied to the class CartonManagerAreaRegistration so that DcmsMobile knows that this route has the capability to handle
        // search results.
        // </para>
        // <para>
        // Attribute <c>Display</c> specifies the most common keywords and name which the user is likely to enter.
        // [Display(ShortName = "CLOC", Name = "Carton Locating")] means that CLOC is an unique keyword and Carton Locating is a name of application for CLOC. Keyword should be different in area and subarea.
        // As a best practice, the keyword
        // should be globally unique. If the user enters an exact match for one of these keywords, your <see cref="GetVirtualPath"/> function must never return null.
        // </para>
        // </remarks>

        [Display(ShortName = "CLOC", Name = "Carton Locating", Description = "Locate cartons in numbered bin locations")]
        [UIHint("mobile", "DcmsMobile")]
        public static RouteValueDictionary Pallet
        {
            get
            {
                return MVC_CartonManager.CartonManager.Locating.Pallet().GetRouteValueDictionary();
            }
        }

        [Display(ShortName = "C2P", Name = "Carton to Pallet", Description = "Place cartons on a pallet")]
        [UIHint("mobile", "DcmsMobile")]
        [UIHint("desktop", "DcmsMobile")]
        public static RouteValueDictionary PalletizeUi
        {
            get
            {
                return MVC_CartonManager.CartonManager.Home.PalletizeUi().GetRouteValueDictionary();
            }
        }

        [Display(ShortName = "MRC", Name = "Mark Rework Complete", Description = "Use this to Mark Rework Complete of those Cartons on which rework has to be done.")]
        [UIHint("mobile", "DcmsMobile")]
        [UIHint("desktop", "DcmsMobile")]
        public static RouteValueDictionary MarkReworkCompleteUi
        {
            get
            {
                return MVC_CartonManager.CartonManager.Home.MarkReworkCompleteUi().GetRouteValueDictionary();
            }
        }

        [Display(ShortName = "ARW", Name = "Abandon Rework", Description = "If you no longer want to perform rework on Cartons which had been marked for rework, use this UI to let the system know")]
        [UIHint("mobile", "DcmsMobile")]
        [UIHint("desktop", "DcmsMobile")]
        public static RouteValueDictionary AbandonReworkUi
        {
            get
            {
                return MVC_CartonManager.CartonManager.Home.AbandonReworkUi().GetRouteValueDictionary();
            }
        }

        [Display(ShortName = "CED", Name = "Carton Editor", Description = "Scan carton to edit")]
        [UIHint("desktop", "DcmsMobile")]
        public static RouteValueDictionary CartonEditorIndex
        {
            get
            {
                return MVC_CartonManager.CartonManager.Home.CartonEditorIndex().GetRouteValueDictionary();
            }
        }

        [Display(ShortName = "BCED", Name = "Bulk Update Cartons", Description = "This is an advanced UI which enables you to update multiple properties of Cartons.You specify the properties you want to update and then scan Cartons whose properties need to be updated.")]
        [UIHint("desktop", "DcmsMobile")]
        public static RouteValueDictionary AdvanceUi
        {
            get
            {
                return MVC_CartonManager.CartonManager.Home.AdvancedUi().GetRouteValueDictionary();
            }
        }
    }
    ///// <summary>
    ///// Enables DcmsMobile to check whether we are prepared to respond to the search text entered by the user
    ///// </summary>
    ///// <remarks>
    ///// <para>
    ///// Attribute <c>[DefaultProperty("search")]</c> must be applied to the class so that DcmsMobile knows that this route has the capability to handle
    ///// search results.
    ///// </para>
    ///// <para>
    ///// Attribute <c>AdditionalMetadata</c> specifies the most common keywords which the user is likely to enter.
    ///// [AdditionalMetadata("keyword", "CLOC, Locating")] means that CLOC is a unique keyword and Locating is a synonym for CLOC. Any number of synonyms can be provided.
    ///// The first snonym is treated as the short description for the keyword.
    ///// Multiple AdditionalMetadata attributes can be specified.
    ///// As a best practice, the keyword
    ///// should be globally unique. If the user enters an exact match for one of these keywords, your <see cref="GetVirtualPath"/> function must never return null.
    ///// </para>
    ///// <para>
    ///// 
    ///// </para>
    ///// </remarks>
    //[AdditionalMetadata("keyword", "CLOC, Carton Locating")]
    //[AdditionalMetadata("keyword", "C2P, Carton to Pallet, Palletize")]
    //[AdditionalMetadata("keyword", "MRC, Mark Rework Complete")]
    //[AdditionalMetadata("keyword", "ARW, Abandon Rework")]
    //[AdditionalMetadata("keyword", "CED, Carton Editor")]
    //[AdditionalMetadata("subarea", new [] {"CLOC", "Carton Locating", "Locate cartons in numbered bin locations"})]
    //internal class SearchRoute : RouteBase
    //{
    //    /// <summary>
    //    /// Should always return null. This ensures that this route will never match any incoming request
    //    /// </summary>
    //    /// <param name="httpContext"></param>
    //    /// <returns></returns>
    //    public override RouteData GetRouteData(HttpContextBase httpContext)
    //    {
    //        return null;
    //    }

    //    /// <summary>
    //    /// Return null if <paramref name="values"/> does not contain a value for <c>search</c>. Else return the URL which should be displayed in search results.
    //    /// </summary>
    //    /// <param name="requestContext"></param>
    //    /// <param name="values">The search text is available against the key <c>search</c>. It is always in upper case.
    //    /// </param>
    //    /// <returns>The URL to display in search results if the search text is treated as a hit. Otherwise it returns null.</returns>
    //    /// <remarks>
    //    /// <para>
    //    ///  DataTokens can contain the following keys:
    //    ///    <c>title</c>. (Required). The value is the text which will be made clickable,
    //    ///    <c>description</c>. (Optional). The value is the text which will be displayed after the link.
    //    ///    <c>platform</c>. By default, the result is not displayed on the ringscanner. Set the value of this key to "mobile" if this result should be displayed on mobile screens.
    //    /// </para>
    //    /// </remarks>
    //    public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
    //    {
    //        object val;
    //        var b = values.TryGetValue("search", out val);
    //        if (!b || val == null || string.IsNullOrWhiteSpace(val.ToString()))
    //        {
    //            return null;
    //        }

    //        // Now we are sure that the passed val is one of our keywords

    //        var url = new UrlHelper(requestContext);
    //        VirtualPathData result = new VirtualPathData(this, null);
    //        switch ((string)val)
    //        {
    //            case "CLOC":
    //                //result = new VirtualPathData(this, );
    //                result.VirtualPath = url.Action(MVC_CartonManager.CartonManager.Locating.Pallet());
    //                result.DataTokens.Add("title", "Locate Cartons");
    //                result.DataTokens.Add("description", "Use this UI to locate cartons on a pallet. Cartons of scanned pallet, which were not located will be marked in suspense.");
    //                result.DataTokens.Add("platform", "mobile");
    //                break;

    //            case "C2P":
    //                result.VirtualPath = url.Action(MVC_CartonManager.CartonManager.Home.PalletizeUi());
    //                result.DataTokens.Add("title", "Create Pallet");
    //                result.DataTokens.Add("description","Scan Cartons and place them on scanned Area and Pallet. You can also merge Pallets here.");
    //                result.DataTokens.Add("platform", "mobile");
    //                break;

    //            case "MRC":
    //                result.VirtualPath = url.Action(MVC_CartonManager.CartonManager.Home.MarkReworkCompleteUi());
    //                result.DataTokens.Add("title", "Mark Rework Complete");
    //                result.DataTokens.Add("description","Use this to Mark Rework Complete of those Cartons on which rework has to be done.");
    //                result.DataTokens.Add("platform", "mobile");
    //                break;

    //            case "ARW":
    //                result.VirtualPath = url.Action(MVC_CartonManager.CartonManager.Home.AbandonReworkUi());
    //                result.DataTokens.Add("title", "Abandon Rework");
    //                result.DataTokens.Add("description", "If you no longer want to perform rework on Cartons which had been marked for rework, use this UI to let the system know");
    //                result.DataTokens.Add("platform", "mobile");
    //                break;

    //            case "CED":
    //                result.VirtualPath = url.Action(MVC_CartonManager.CartonManager.Home.CartonEditorIndex());
    //                result.DataTokens.Add("description", "Scan Carton to Edit");
    //                result.DataTokens.Add("title", "Carton Editor");
    //                break;
    //            default:
    //                throw new NotSupportedException(val.ToString());
    //        }
    //        return result;
    //    }
    //}
}
