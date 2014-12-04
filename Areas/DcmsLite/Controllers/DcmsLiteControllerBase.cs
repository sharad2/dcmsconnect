using DcmsMobile.DcmsLite.Repository;
using DcmsMobile.DcmsLite.ViewModels;
using EclipseLibrary.Mvc.Controllers;
using System;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers
{
    [RouteArea("DcmsLite")]
    public class DcmsLiteControllerBase<TService> : EclipseController where TService : DcmsLiteServiceBase, new()
    {
        protected TService _service;

        protected string _buildingId;

        protected const string ROLE_DCMS_LITE = "DCMSLITE_MANAGER";

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (_service == null)
            {
                var connectString = ConfigurationManager.ConnectionStrings["dcmslite"].ConnectionString;
                var userName = requestContext.HttpContext.SkipAuthorization ? string.Empty : requestContext.HttpContext.User.Identity.Name;
                var clientInfo = string.IsNullOrEmpty(requestContext.HttpContext.Request.UserHostName) ? requestContext.HttpContext.Request.UserHostAddress :
                    requestContext.HttpContext.Request.UserHostName;
                _service = new TService();
                _service.Initialize(requestContext.HttpContext.Trace, connectString, userName, clientInfo);
            }
            _buildingId = ConfigurationManager.AppSettings["DcmsLite.WarehouseLocation"];
        }

        protected override void Dispose(bool disposing)
        {
            if (_service != null)
            {
                _service.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var vr = filterContext.Result as ViewResult;
            if (vr != null)
            {
                var model = (ViewModelBase)vr.Model;
                if (model == null)
                {
                    throw new ApplicationException("Not expecting model to be null");
                }
                model.BuildingId = _buildingId;
                model.BuildingDescription = _service.GetBuildingDescription(model.BuildingId);
                model.IsEditable = AuthorizeExAttribute.IsSuperUser(HttpContext) || HttpContext.User.IsInRole(ROLE_DCMS_LITE);
                model.DcmsLiteRoleName = ROLE_DCMS_LITE;
            }
            base.OnActionExecuted(filterContext);
        }
    }
}