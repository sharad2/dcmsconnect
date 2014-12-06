using System;
using System.Linq;
using System.Web.Mvc;
using DcmsMobile.CartonManager.Models;
using DcmsMobile.CartonManager.Repository;
using EclipseLibrary.Mvc.Html;

namespace DcmsMobile.CartonManager.Areas.CartonManager.Controllers
{
    public partial class AutoCompleteController : Controller
    {
        private object Map(Sku sku)
        {
            return new
            {
                label = string.Format("{0},{1},{2},{3}", sku.Style, sku.Color, sku.Dimension, sku.SkuSize),
                value = sku.SkuId.ToString(),
                shortName = sku.UpcCode
            };
        }

        #region Intialization

        public AutoCompleteController()
        {

        }

        private AutoCompleteRepository _repos;

        protected override void Initialize(System.Web.Routing.RequestContext ctx)
        {
            var module = ctx.HttpContext.Request.Url == null ? "CartonManager" : ctx.HttpContext.Request.Url.AbsoluteUri;
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
            return Json(data.Select(p => Map(p)), JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Called by the remote validator
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// The name of the posted field name is not predictable. So we simply use the first form value.
        /// The method must be called via GET since we rely on the value to get passed via query string
        /// </remarks>
        [HttpGet]
        public virtual JsonResult ValidateSku()
        {
            if (Request.QueryString.Count == 0)
            {
                throw new ApplicationException("Nothing to validate");
            }
            var barCode = Request.QueryString[0].ToUpper();
            var sku = _repos.GetSkuFromUpc(barCode);
            if (sku == null)
            {
                throw new ApplicationException(string.Format("No such SKU: {0}", barCode.ToUpper()));
            }
            return Json(Map(sku), JsonRequestBehavior.AllowGet);
        }

    }
}



//$Id$