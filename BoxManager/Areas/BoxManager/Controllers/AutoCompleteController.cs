using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using DcmsMobile.BoxManager.Repository;

namespace DcmsMobile.BoxManager.Areas.BoxManager.Controllers
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
            var clientInfo = string.IsNullOrEmpty(ctx.HttpContext.Request.UserHostName) ?
                             ctx.HttpContext.Request.UserHostAddress : ctx.HttpContext.Request.UserHostName;
            var userName = ctx.HttpContext.SkipAuthorization ? string.Empty : ctx.HttpContext.User.Identity.Name;
            var connectString = ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString;
            _repos = new AutoCompleteRepository(ctx.HttpContext.Trace, connectString, userName, clientInfo);

            base.Initialize(ctx);
        }

        protected override void Dispose(bool disposing)
        {
            _repos.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        public class AutocompleteItem
        {
            /// <summary>
            /// Text displayed in the list
            /// </summary>
            public string label { get; set; }

            /// <summary>
            /// The id which is posted back (e.g. SKU Id)
            /// </summary>
            public string value { get; set; }
        }

        private AutocompleteItem MapCustomer(Customer customer)
        {
            return new AutocompleteItem
            {
                label = string.Format("{0}:{1}", customer.CustomerId, customer.CustomerName),
                value = customer.CustomerId
            };
        }

        /// <summary>
        /// method for Autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public virtual ActionResult CustomerAutocomplete(string term)
        {
            var data = _repos.CustomerAutoComplete(term).Select(p => MapCustomer(p));
            return Json(data, JsonRequestBehavior.AllowGet);
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
        public virtual JsonResult ValidateCustomer()
        {
            var term = this.Request.QueryString[0];
            if (string.IsNullOrEmpty(term))
            {
                throw new ApplicationException("Internal error. The id to validate was not passed.");
            }
            var data = _repos.GetCustomer(term);
            if (data == null)
            {
                return Json("Invalid customer " + term, JsonRequestBehavior.AllowGet);
            }
            return Json(MapCustomer(data), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public virtual JsonResult GetLabels()
        {
            var term = this.Request.QueryString[0];
            if (string.IsNullOrEmpty(term))
            {
                throw new ApplicationException("Internal error. The id to validate was not passed.");
            }
            var list = from label in _repos.GetLabels(term)
                       select new AutocompleteItem
                           {
                               label = label.Description,
                               value = label.LabelId
                           };
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}
