using DcmsMobile.Repack.Models;
using DcmsMobile.Repack.Repository;
using System;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.Repack.Areas.Repack.Controllers
{
    public partial class AutoCompleteController : Controller
    {
        #region Intialization

        public AutoCompleteController()
        {

        }

        private AutoCompleteRepository _repos;

        protected override void Initialize(System.Web.Routing.RequestContext ctx)
        {
            const string MODULE = "Repack";
            var clientInfo = string.IsNullOrEmpty(ctx.HttpContext.Request.UserHostName) ?
                             ctx.HttpContext.Request.UserHostAddress : ctx.HttpContext.Request.UserHostName;

            _repos = new AutoCompleteRepository(ctx.HttpContext.User.Identity.Name, MODULE, clientInfo, ctx.HttpContext.Trace);

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
            var data = _repos.UpcAutoComplete((term ?? string.Empty).ToUpper());
            //return Json(Mapper.Map<IEnumerable<AutocompleteItem>>(data), JsonRequestBehavior.AllowGet);
            return Json(data.Select(sku => new
            {
                label = sku.Style + "," + sku.Color + "," + sku.Dimension + "," + sku.SkuSize,
                value = sku.UpcCode,
                standardSkuSize = sku.StandardSkuSize
            }), JsonRequestBehavior.AllowGet);
        }
    }
}



//$Id$