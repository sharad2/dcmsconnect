using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.PickWaves.Areas.PickWaves
{
    public class SubAreas
    {
        // <summary>
        // Enables DcmsMobile to check whether we are prepared to respond to the search text entered by the user
        // </summary>
        // <remarks>
        // <para>
        // Attribute <c>[MetadataType(typeof(SubAreas))]</c> must be applied to the class PickWavesAreaRegistration so that DcmsMobile knows that this route has the capability to handle
        // search results.
        // </para>
        // <para>
        // Attribute <c>Display</c> specifies the most common keywords and name which the user is likely to enter.
        // [Display(ShortName = "WAV", Name = "Manage Pick Waves")] means that WAV is an unique keyword and Manage Pick Waves is a name of application for WAV. Keyword should be different in area and subarea.
        // As a best practice, the keyword
        // should be globally unique. If the user enters an exact match for one of these keywords, your <see cref="GetVirtualPath"/> function must never return null.
        // </para>
        // </remarks>

        [Display(ShortName = "WAV", Name = "Manage Pick Waves", Description = "Create and Manage Pick Waves.")]
        [UIHint("desktop", "DcmsMobile")]
        public static RouteValueDictionary WavIndex
        {
            get { return MVC_PickWaves.PickWaves.Home.Index().GetRouteValueDictionary(); }
        }


        [Display(ShortName = "BPP", Name = "Create Box Pick Pallet", Description = "Create pallet for box picking.")]
        [UIHint("desktop", "DcmsMobile")]
        public static RouteValueDictionary BppIndex
        {
            get { return MVC_PickWaves.PickWaves.BoxPickPallet.Index().GetRouteValueDictionary(); }
        }
    }
}
