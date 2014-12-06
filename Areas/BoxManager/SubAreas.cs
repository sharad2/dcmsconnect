using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.BoxManager.Helpers;

namespace DcmsMobile.BoxManager.Areas.BoxManager
{
    public class SubAreas
    {
        // <summary>
        // Enables DcmsMobile to check whether we are prepared to respond to the search text entered by the user
        // </summary>
        // <remarks>
        // <para>
        // Attribute <c>[MetadataType(typeof(SubAreas))]</c> must be applied to the class BoxManagerAreaRegistration so that DcmsMobile knows that this route has the capability to handle
        // search results.
        // </para>
        // <para>
        // Attribute <c>Display</c> specifies the most common keywords and name which the user is likely to enter.
        // [Display(ShortName = "SCM", Name = "Box Editor")] means that SCM is an unique keyword and Box Editor is a name of application for SCM. Keyword should be different in area and subarea.
        // As a best practice, the keyword
        // should be globally unique. If the user enters an exact match for one of these keywords, your <see cref="GetVirtualPath"/> function must never return null.
        // </para>
        // </remarks>
        [Display(ShortName = "STP", Name = "Scan to Pallet", Description = "Creating a pallet in Dock area.", Order = 10)]
        [UIHint("mobile", "DcmsMobile")]
        public static RouteValueDictionary ScanToPallet
        {
            get { return MVC_BoxManager.BoxManager.Home.CreatingPalletIndex().AddRouteValue(AuthorizeExUiAttribute.NAME_UITYPE, UiType.ScanToPallet).GetRouteValueDictionary(); }
        }

        [Display(ShortName = "MP", Name = "Move to Pallet", Description = "Move completed pallets from one location to another.", Order = 20)]
        [UIHint("mobile", "DcmsMobile")]
        public static RouteValueDictionary MovePalletIndex
        {
            get { return MVC_BoxManager.BoxManager.MovePallet.Index().GetRouteValueDictionary(); }
        }

        //[Display(ShortName = "SCM", Name = "Box Editor", Description = "Edit Box properties.", Order = 30)]
        //[UIHint("desktop", "DcmsMobile")]
        //public static RouteValueDictionary BoxEditorIndex
        //{
        //    get { return MVC_BoxManager.BoxManager.BoxEditor.Index().GetRouteValueDictionary(); }
        //}

        [Display(ShortName = "V2P", Name = "Palletize for VAS", Description = "Palletize boxes for VAS", Order = 40)]
        [UIHint("mobile", "DcmsMobile")]
        public static RouteValueDictionary VasIndex
        {
            get { return MVC_BoxManager.BoxManager.Home.CreatingPalletIndex().AddRouteValue(AuthorizeExUiAttribute.NAME_UITYPE, UiType.Vas).GetRouteValueDictionary(); }
        }

        [Display(ShortName = "V2PCONFIG", Name = "VAS Configuration", Description = "Configuration UI for VAS", Order = 50)]
        [UIHint("desktop", "DcmsMobile")]
        public static RouteValueDictionary VasConfigurationIndex
        {
            get { return MVC_BoxManager.BoxManager.VasConfiguration.Index().GetRouteValueDictionary(); }
        }

        //[Display(ShortName = "VAL", Name = "Box Validation", Description = "Validation UI for Box", Order = 60)]
        //[UIHint("desktop", "DcmsMobile")]
        //public static RouteValueDictionary ValidationIndex
        //{
        //    get { return MVC_BoxManager.BoxManager.Validation.Index().GetRouteValueDictionary(); }
        //}

        public static IEnumerable<Tuple<DisplayAttribute, string, bool>> GetMetaData(UrlHelper helper)
        {
            var query = from method in typeof(SubAreas).GetProperties()
                        let displayAttr = method.GetCustomAttributes(typeof(DisplayAttribute), false)
                            .Cast<DisplayAttribute>()
                            .FirstOrDefault()
                        where displayAttr != null
                        orderby displayAttr.GetOrder()
                        select Tuple.Create(displayAttr,
                            helper.RouteUrl((RouteValueDictionary)method.GetValue(null, null)),
                            method.GetCustomAttributes(typeof(UIHintAttribute), false)
                                .Cast<UIHintAttribute>().Any(p => p.UIHint == "mobile"))
                        ;
            return query;
        }
    }

}
