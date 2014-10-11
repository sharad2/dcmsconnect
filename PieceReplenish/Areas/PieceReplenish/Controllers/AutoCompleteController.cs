using System.Linq;
using System.Web.Mvc;
using DcmsMobile.PieceReplenish.Repository;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.PieceReplenish.Areas.PieceReplenish.Controllers
{
    public partial class AutoCompleteController : EclipseController
    {
        private class Autocomplete
        {
            /// <summary>
            /// Text displayed in the list
            /// </summary>
            public string label { get; set; }

            /// <summary>
            /// The id which is posted back (e.g. SKU Id)
            /// </summary>
            public string value { get; set; }

            /// <summary>
            /// Friendly short name of the selected value (such as UPC code). Defaults to value
            /// </summary>
            public string shortName { get; set; }
        }

        private Autocomplete Map(Sku src)
        {
            return new Autocomplete
            {
                label = string.Format("{0},{1},{2},{3}", src.Style, src.Color, src.Dimension, src.SkuSize),
                value = src.SkuId.ToString(),
                shortName = src.UpcCode
            };
        }
        #region Intialization

        public AutoCompleteController()
        {

        }
        private AutoCompleteRepository _repos;

        protected override void Initialize(System.Web.Routing.RequestContext ctx)
        {
            var module = ctx.HttpContext.Request.Url == null ? "PieceReplenish" : ctx.HttpContext.Request.Url.AbsoluteUri;
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

        ///// <summary>
        ///// Called by the remote validator
        ///// </summary>
        ///// <returns></returns>
        ///// <remarks>
        ///// The name of the posted field name is not predictable. So we simply use the first form value.
        ///// The method must be called via GET since we rely on the value to get passed via query string
        ///// </remarks>
        //[HttpGet]
        //public virtual JsonResult ValidateSku()
        //{
        //    var barCode = this.Request.QueryString[0].ToUpper();
        //    if (string.IsNullOrEmpty(barCode))
        //    {
        //        throw new ApplicationException("Internal error. The id to validate was not passed.");
        //    }
        //    var data = _repos.GetSkuFromUpc(barCode);
        //    if (data == null)
        //    {
        //        return Json("Invalid SKU " + barCode, JsonRequestBehavior.AllowGet);
        //    }
        //    return Json(Map(data), JsonRequestBehavior.AllowGet);
        //}
    }
}
