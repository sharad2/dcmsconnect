using System.Linq;
using System.Web.Mvc;
using DcmsMobile.REQ2.Repository;

namespace DcmsMobile.REQ2.Areas.REQ2.Controllers
{
    public partial class AutoCompleteController : Controller
    {
        //static AutoCompleteController()
        //{
        //    Mapper.CreateMap<SkuModel, AutocompleteItem>()
        //        .ForMember(dest => dest.label, opt => opt.MapFrom(src => string.Format("{0},{1},{2},{3}", src.Style, src.Color, src.Dimension, src.SkuSize)))
        //        .ForMember(dest => dest.value, opt => opt.MapFrom(src => src.SkuId))
        //        .ForMember(dest => dest.shortName, opt => opt.MapFrom(src => src.UpcCode))
        //        ;
        //}

        #region Intialization

        public AutoCompleteController()
        {

        }

        private AutoCompleteRepository _repos;

        protected override void Initialize(System.Web.Routing.RequestContext ctx)
        {
            string module = "REQ2";
            var clientInfo = string.IsNullOrEmpty(ctx.HttpContext.Request.UserHostName) ?
                             ctx.HttpContext.Request.UserHostAddress : ctx.HttpContext.Request.UserHostName;

            _repos = new AutoCompleteRepository(ctx.HttpContext.User.Identity.Name, module, clientInfo, ctx.HttpContext.Trace);

            base.Initialize(ctx);
        }

        protected override void Dispose(bool disposing)
        {
            _repos.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        /// method for Autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public virtual ActionResult SkuAutocomplete(string term)
        {
            var data = _repos.UpcAutoComplete(term.ToUpper());
            return Json(data.Select(p => new {
                label = string.Format("{0},{1},{2},{3}", p.Style, p.Color, p.Dimension, p.SkuSize),
                value = p.Style,
                color = p.Color,
                dimension = p.Dimension,
                skusize = p.SkuSize
            }), JsonRequestBehavior.AllowGet);
        }


        ///// <summary>
        ///// Called by the remote validator
        ///// </summary>
        ///// <returns></returns>
        ///// <remarks>
        ///// The name of the posted field name is not predictable. So we simply use the first form value.
        ///// The method must be called via GET since we rely on the value to get passed via query string
        ///// </remarks>
        //[EclipseLibrary.Mvc.Controllers.HandleAjaxError(true)]
        //[HttpGet]
        //public virtual JsonResult ValidateSku()
        //{
        //    if (Request.QueryString.Count == 0)
        //    {
        //        throw new ApplicationException("Nothing to validate");
        //    }
        //    var barCode = Request.QueryString[0].ToUpper();
        //    var sku = _repos.GetSkuFromUpc(barCode);
        //    if (sku == null)
        //    {
        //        throw new ApplicationException(string.Format("No such SKU: {0}", barCode.ToUpper()));
        //    }
        //    return Json(Mapper.Map<AutocompleteItem>(sku), JsonRequestBehavior.AllowGet);
        //}

    }
}

//$Id$
