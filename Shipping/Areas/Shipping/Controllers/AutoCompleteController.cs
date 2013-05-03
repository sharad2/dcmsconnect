using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using DcmsMobile.Shipping.Repository;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.Shipping.Areas.Shipping.Controllers
{
    public partial class AutoCompleteController : EclipseController
    {

        private class AutoCompleteItem
        {
            public string label { get; set; }

            public string value { get; set; }
        }


        #region Intialization

        public AutoCompleteController()
        {

        }

        private AutoCompleteRepository _repos;

        protected override void Initialize(System.Web.Routing.RequestContext ctx)
        {
            //var module = ctx.HttpContext.Request.Url == null ? "Shipping" : ctx.HttpContext.Request.Url.AbsoluteUri;
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

        #region Customer AutoComplete

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
            //return Json(Mapper.Map<AutocompleteItem>(data), JsonRequestBehavior.AllowGet);
            return Json(MapCustomer(data), JsonRequestBehavior.AllowGet);
        }

        private AutoCompleteItem MapCustomer(Customer customer)
        {
            return new AutoCompleteItem
            {
                label = string.Format("{0}:{1}", customer.CustomerId, customer.CustomerName),
                value = customer.CustomerId
            };
        }

        #endregion

        #region Carrier AutoComplete

       

        /// <summary>
        /// Get matching carriers
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public virtual JsonResult GetCarriers(string term)
        {
            var data = _repos.GetCarriers(term, null).Select(p => Map(p)); ;
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult ValidateCarrier(string term)
        {
            //var term = this.Request.QueryString[0];
            if (string.IsNullOrEmpty(term))
            {
                throw new ApplicationException("Internal error. The id to validate was not passed.");
            }
            var data = _repos.GetCarriers(null, term).SingleOrDefault();
            if (data == null)
            {
                Response.StatusCode = 203;
                return Content(string.Format("Invalid carrier {0}", term));
            }
            return Content("");
        }

        private AutoCompleteItem Map(Carrier src)
        {
            return new AutoCompleteItem
            {
                label = string.Format("{0} - {1}", src.CarrierId, src.Description),
                value = src.CarrierId
            };
        }

        #endregion

    }
}



//$Id$
