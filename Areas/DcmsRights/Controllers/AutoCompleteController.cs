using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using EclipseLibrary.Mvc.Html;

namespace DcmsMobile.DcmsRights.Areas.DcmsRights.Controllers
{
    public partial class AutoCompleteController : Controller
    {
        /// <summary>
        /// Required for T4MVC.
        /// </summary>
        public AutoCompleteController()
        { }

        //static AutoCompleteController()
        //{
        //    Mapper.CreateMap<MembershipUser, AutocompleteItem>()
        //            .ForMember(dest => dest.label, opt => opt.MapFrom(src => src.UserName))
        //            .ForMember(dest => dest.value, opt => opt.MapFrom(src => src.UserName));
        //}
        /// <summary>
        /// method for Users Autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public virtual ActionResult UserAutocomplete(string term)
        {
            try
            {
                int totalRecords;
                var users = Membership.FindUsersByName("%" + term + "%", 0, int.MaxValue, out totalRecords);
                //return Json(Mapper.Map<IEnumerable<AutocompleteItem>>(users), JsonRequestBehavior.AllowGet);
                return Json(users.Cast<MembershipUser>().Select(p => new 
                {
                    label = p.UserName,
                    value = p.UserName
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new[] { new  {
                     label = ex.Message
                }}, JsonRequestBehavior.AllowGet);
            }
        }
    }
}