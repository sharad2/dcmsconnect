using DcmsMobile.Receiving.Areas.Receiving.Home.Repository;
using DcmsMobile.Receiving.Models;
using DcmsMobile.Receiving.Repository;
using DcmsMobile.Receiving.ViewModels;
using EclipseLibrary.Mvc.Controllers;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.Receiving.Areas.Receiving.Controllers
{
    public partial class AutoCompleteController : EclipseController
    {

        //private AutocompleteItem Map(Carrier src)
        //{
        //    return new AutocompleteItem
        //    {
        //       label = string.Format("{0}: {1}", src.CarrierId, src.Description),
        //       value = src.CarrierId
        //    };
        //}

        public AutoCompleteController()
        {

        }

        private AutocompleteRepository _repos;

        protected override void Initialize(RequestContext requestContext)
        {

            _repos = new AutocompleteRepository(requestContext);

            base.Initialize(requestContext);
        }

        protected override void Dispose(bool disposing)
        {
            _repos.Dispose();
            base.Dispose(disposing);
        }

        #region Autocomplete

        /// <summary>
        /// Get matching carriers
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public virtual JsonResult GetCarriers(string term)
        {
            // Change null to empty string
            term = term ?? string.Empty;

            var tokens = term.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            string searchId;
            string searchDescription;

            switch (tokens.Count)
            {
                case 0:
                    // All carriers
                    searchId = searchDescription = string.Empty;
                    break;

                case 1:
                    // Try to match term with either id or description
                    searchId = searchDescription = tokens[0];
                    break;

                case 2:
                    // Try to match first token with id and second with description
                    searchId = tokens[0];
                    searchDescription = tokens[1];
                    break;

                default:
                    // For now, ignore everything after the second :
                    searchId = tokens[0];
                    searchDescription = tokens[1];
                    break;


            }

            var data = _repos.GetCarriers(searchId, searchDescription).Select(p => new
            {
                label = string.Format("{0}: {1}", p.Item1, p.Item2),
                value = p.Item1
            }); ;
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returning Json result for Style Autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>       
        public virtual JsonResult StyleAutocomplete(string term)
        {
            term = term ?? string.Empty;

            var tokens = term.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            string searchId;
            string searchDescription;

            switch (tokens.Count)
            {
                case 0:
                    // All styles
                    searchId = searchDescription = string.Empty;
                    break;

                case 1:
                    // Try to match term with either id or description
                    searchId = searchDescription = tokens[0];
                    break;

                case 2:
                    // Try to match first token with id and second with description
                    searchId = tokens[0];
                    searchDescription = tokens[1];
                    break;

                default:
                    // For now, ignore everything after the second :
                    searchId = tokens[0];
                    searchDescription = tokens[1];
                    break;


            }
            var data = _repos.GetStyles(searchId, searchDescription).Select(p => new
            {
                label = string.Format("{0}: {1}", p.Item1,p.Item2),
                value = p.Item1
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// validate selected style
        /// </summary>
        /// <returns></returns>
        [Obsolete("We are validating using new way of bootstrap for empty results called templates(empty:'result empty text here ')}")]
        public virtual JsonResult ValidateStyle()
        {
            var term = this.Request.QueryString[0];
            if (string.IsNullOrEmpty(term))
            {
                throw new ApplicationException("Internal error. The id to validate was not passed.");
            }
            var data = _repos.GetStyles(null, term.ToUpper()).Select(p => new AutocompleteItem()
            {
                label = string.Format("{0}: {1}", p.Item1, p.Item2),
                value = p.Item1
            }).FirstOrDefault();
            if (data == null)
            {
                return Json("Invalid Style " + term, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Returning Json result for Style Autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public virtual JsonResult ColorAutocomplete(string term)
        {
            //var data = Mapper.Map<IEnumerable<AutocompleteItem>>(_repos.GetStyles(term.ToUpper()));
            var data = _repos.GetColors(term.ToUpper(), null).Select(p => new AutocompleteItem()
            {
                label = string.Format("{0}: {1}", p.ColorId, p.Description),
                value = p.ColorId
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// validate selected color
        /// </summary>
        /// <returns></returns>
        public virtual JsonResult ValidateColor()
        {
            var term = this.Request.QueryString[0];
            if (string.IsNullOrEmpty(term))
            {
                throw new ApplicationException("Internal error. The id to validate was not passed.");
            }
            var data = _repos.GetColors(null, term.ToUpper()).Select(p => new AutocompleteItem()
             {
                 label = string.Format("{0}: {1}", p.ColorId, p.Description),
                 value = p.ColorId
             }).FirstOrDefault();
            if (data == null)
            {
                return Json("Invalid Color " + term, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

    }
}


//$Id$