using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.PickWaves.Repository;
using DcmsMobile.PickWaves.Repository.Config;
using DcmsMobile.PickWaves.ViewModels;
using EclipseLibrary.Mvc.Controllers;


namespace DcmsMobile.PickWaves.Areas.PickWaves.Controllers
{
    //public partial class AutoCompleteController : EclipseController
    //{
    //    #region Initialization
    //    public AutoCompleteController()
    //    {

    //    }

    //    private AutoCompleteRepository _repos;

    //    protected override void Initialize(RequestContext ctx)
    //    {
    //        base.Initialize(ctx);
    //        _repos = new AutoCompleteRepository(this.HttpContext.Trace, Request.UserHostName ?? Request.UserHostAddress);
    //    }

    //    protected override void Dispose(bool disposing)
    //    {
    //        _repos.Dispose();
    //        base.Dispose(disposing);
    //    }

    //    #endregion

    //    private AutocompleteItem MapCustomer(Customer customer)
    //    {
    //        return new AutocompleteItem
    //        {
    //            label = string.Format("{0}:{1}", customer.CustomerId, customer.Name),
    //            value = customer.CustomerId
    //        };
    //    }

    //    private AutocompleteItem MapStyle(Style style)
    //    {
    //        return new AutocompleteItem
    //        {
    //            label = string.Format("{0}:{1}", style.StyleId, style.Description),
    //            value = style.StyleId
    //        };
    //    }

    //    /// <summary>
    //    /// method for Autocomplete
    //    /// </summary>
    //    /// <param name="term"></param>
    //    /// <returns></returns>
    //    public virtual ActionResult CustomerAutocomplete(string term)
    //    {
    //        var data = _repos.CustomerAutoComplete(term).Select(p => MapCustomer(p));
    //        return Json(data, JsonRequestBehavior.AllowGet);
    //    }

    //    public virtual ActionResult StyleAutoComplete(string term)
    //    {
    //        var data = _repos.StyleAutoComplete(term,null).Select(p => MapStyle(p));
    //        return Json(data, JsonRequestBehavior.AllowGet);
    //    }



    //    /// <summary>
    //    /// Called by the remote validator
    //    /// </summary>
    //    /// <returns></returns>
    //    /// <remarks>
    //    /// The name of the posted field name is not predictable. So we simply use the first form value.
    //    /// The method must be called via GET since we rely on the value to get passed via query string
    //    /// </remarks>
    //    [HttpGet]
    //    public virtual JsonResult ValidateCustomer()
    //    {
    //        var term = this.Request.QueryString[0];
    //        if (string.IsNullOrEmpty(term))
    //        {
    //            throw new ApplicationException("Internal error. The id to validate was not passed.");
    //        }
    //        var data = _repos.GetCustomer(term);
    //        if (data == null)
    //        {
    //            return Json("Invalid customer " + term, JsonRequestBehavior.AllowGet);
    //        }
    //        return Json(MapCustomer(data), JsonRequestBehavior.AllowGet);
    //    }

    //    /// <summary>
    //    /// Called by the remote validator
    //    /// </summary>
    //    /// <returns></returns>
    //    /// <remarks>
    //    /// The name of the posted field name is not predictable. So we simply use the first form value.
    //    /// The method must be called via GET since we rely on the value to get passed via query string
    //    /// </remarks>
    //    [HttpGet]
    //    public virtual JsonResult ValidateStyle()
    //    {
    //        var term = this.Request.QueryString[0];
    //        if (string.IsNullOrEmpty(term))
    //        {
    //            throw new ApplicationException("Internal error. The Style to validate was not passed.");
    //        }
    //        var data = _repos.StyleAutoComplete(null, term.ToUpper()).Select(p => new AutocompleteItem()
    //        {
    //            label = string.Format("{0} - {1}", p.StyleId, p.Description),
    //            value = p.StyleId
    //        }).FirstOrDefault();
    //        if (data == null)
    //        {
    //            return Json("Invalid Style " + term, JsonRequestBehavior.AllowGet);
    //        }
    //        return Json(data, JsonRequestBehavior.AllowGet);
    //    }
    //}
}