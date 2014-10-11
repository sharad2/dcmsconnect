using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.PieceReplenish.Areas.PieceReplenish
{
    public class SubAreas
    {
        // <summary>
        // Enables DcmsMobile to check whether we are prepared to respond to the search text entered by the user
        // </summary>
        // <remarks>
        // <para>
        // Attribute <c>[MetadataType(typeof(SubAreas))]</c> must be applied to the class PieceReplenishAreaRegistration so that DcmsMobile knows that this route has the capability to handle
        // search results.
        // </para>
        // <para>
        // Attribute <c>Display</c> specifies the most common keywords and name which the user is likely to enter.
        // [Display(ShortName = "PUL", Name = "Replenishment Pulling")] means that PUL is an unique keyword and Replenishment Pulling is a name of application for PUL. Keyword should be different in area and subarea.
        // As a best practice, the keyword
        // should be globally unique. If the user enters an exact match for one of these keywords, your <see cref="GetVirtualPath"/> function must never return null.
        // </para>
        // </remarks>

        [Display(ShortName = "PUL", Name = "Replenishment Pulling", Description = "Manages Forward Pick replenishment. Includes guided mobile pulling of cartons for restock areas.")]
        [UIHint("mobile", "DcmsMobile")]
        [UIHint("desktop", "DcmsMobile")]
        public static RouteValueDictionary Building
        {
            get { return MVC_PieceReplenish.PieceReplenish.Home.Actions.Building().GetRouteValueDictionary(); }
        }

        [Display(ShortName = "RST", Name = "Restock", Description = "Restock Carton is used to restock carton’s SKU on a location where carton’s SKU is assigned.")]
        [UIHint("mobile", "DcmsMobile")]
        public static RouteValueDictionary BuildingRestock
        {
            get
            {
                return MVC_PieceReplenish.PieceReplenish.Restock.Carton().GetRouteValueDictionary();
            }
        }
    }
}
