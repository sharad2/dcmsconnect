using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.CartonManager.Areas.CartonManager
{
    [MetadataType(typeof(SubAreas))]
    public class CartonManagerAreaRegistration : AreaRegistration
    {
        /// <summary>
        /// Attributes have been applied to integrate with the DcmsMobile menu system
        /// </summary>
        /// <remarks>
        /// <para>
        /// Display - Name : Name of the area displayed in the main menu of DcmsMobile
        /// Display - Order : Controls the placement in the menu list. 100 is normal. Give a higher number if you want to be towards the end of the list.
        /// Display - Description : Description of the area displayed in the main menu of DcmsMobile
        /// Display - ShortName : If the user searches for this, it is equivalent to selecting the area from the menu.
        /// UIHint("mobile", "DcmsMobile") - Indicates that the mobile menu should display this area
        /// [UIHint("desktop", "DcmsMobile")] - Indicates that this area has some screens optimized for the desktop. The area is always displayed in the desktop menu, whether or not
        ///   this attribute is provided.
        /// </para>
        /// </remarks>
        [Display(Name = "Carton Manager", Order = 100, Description = "Formerly called Carton Editor. Edit carton properties; Move carton from one area to another; Change carton location.",
            ShortName="CTN")]
        [UIHint("mobile", "DcmsMobile")]
        [UIHint("desktop", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "CartonManager";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "CartonManager_default",
                this.AreaName + "/{controller}/{action}/{id}",
                new
                {
                    controller = MVC_CartonManager.CartonManager.Home.Name,
                    action = MVC_CartonManager.CartonManager.Home.ActionNames.Index,
                    id = UrlParameter.Optional
                },
                new[] { typeof(Controllers.HomeController).Namespace }
            );

            //var route = new SearchRoute();
            //context.Routes.Add(route);
        }

    }
}
//$Id$
