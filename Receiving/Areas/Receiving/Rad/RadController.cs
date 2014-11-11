using DcmsMobile.Receiving.Models.Rad;
using DcmsMobile.Receiving.Repository;
using DcmsMobile.Receiving.ViewModels.Rad;
using EclipseLibrary.Mvc.Controllers;
using EclipseLibrary.Mvc.Html;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;


namespace DcmsMobile.Receiving.Areas.Receiving.Controllers
{
    public partial class RadController : EclipseController
    {
        private const string ROLE_RAD_EDITING = "SRC_RECEIVING_MGR";

        private SpotCheckViewModel Map(SpotCheckConfiguration src)
        {
            return new SpotCheckViewModel
                {
                    Style = src.Style == "." ? "All" : src.Style,
                    SewingPlantId = src.SewingPlantId == "." ? "All" : src.SewingPlantId,
                    PlantName = src.PlantName,
                    SpotCheckPercent = src.SpotCheckPercent,
                    Color = src.Color == "." ? "All" : src.Color,
                    IsSpotCheckEnabled = src.IsSpotCheckEnable.Value,
                    CreatedDate = src.CreatedDate,
                    CreatedBy = src.CreatedBy,
                    ModifiedDate = src.ModifiedDate,
                    ModifiedBy = src.ModifiedBy
                };

        }
        private GroupSelectListItem Map(SewingPlant src)
        {
            return new GroupSelectListItem
            {
                Text = src.SewingPlantCode + ": " + src.PlantName,
                Value = src.SewingPlantCode,
                GroupText = src.GroupingColumn + ":" + src.CountryName
            };
        }
        private SpotCheckConfiguration Map(SpotCheckViewModel src)
        {
            return new SpotCheckConfiguration
                {
                    Style = string.IsNullOrEmpty(src.Style) || src.Style == "All" ? "." : src.Style,
                    SewingPlantId = string.IsNullOrEmpty(src.SewingPlantId) || src.SewingPlantId == "All" ? "." : src.SewingPlantId,
                    SpotCheckPercent = src.SpotCheckPercent,
                    PlantName = src.PlantName,
                    Color = string.IsNullOrEmpty(src.Color) || src.Color == "All" ? "." : src.Color,
                    IsSpotCheckEnable = src.IsSpotCheckEnabled
                };
        }

        private SpotCheckAreaModel Map(SpotCheckArea src)
        {
            return new SpotCheckAreaModel
            {
                AreaId = src.AreaId,
                BuildingId = src.BuildingId
            };
        }
        /// <summary>
        /// Required by T4MVC
        /// </summary>
        // ReSharper disable UnusedMember.Global
        public RadController()
        // ReSharper restore UnusedMember.Global
        {

        }

        private RadService _service;

        protected override void Initialize(RequestContext requestContext)
        {
            if (_service == null)
            {
                _service = new RadService(requestContext);
            }
            base.Initialize(requestContext);
        }

        public virtual ActionResult Index()
        {
            var model = new RadViewModel();
            var sc = _service.GetSpotCheckList();
            model.SpotCheckList = sc.Select(p => Map(p)).ToList();
            var plantlist = _service.GetSewingPlants().Select(p => Map(p));

            model.SpotCheckViewModel = new SpotCheckViewModel
                {
                    SewingPlantList = plantlist
                };

            model.EnableEditing = AuthorizeExAttribute.IsSuperUser(HttpContext) || this.HttpContext.User.IsInRole(ROLE_RAD_EDITING);
            model.SpotCheckAreaList = _service.GetSpotCheckAreas().Select(p => Map(p)).ToList();
            ViewBag.EnableEditing = model.EnableEditing;

            return View(Views.Index, model);
        }

        /// <summary>
        /// Adds/Update SpotCheck Percentage for given style
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Updating Receiving Configuration {0}", Roles = ROLE_RAD_EDITING)]
        public virtual ActionResult SetSpotCheckPercentage(SpotCheckViewModel model)
        {
            if (!model.AllStyles.HasValue && string.IsNullOrEmpty(model.Style))
            {
                this.Response.StatusCode = 203;
                return Content("Please provide value for style.");

            }
            if (model.SpotCheckPercent == null)
            {
                this.Response.StatusCode = 203;
                return Content("Please provide value for spotcheck %");

            }
            if (!model.AllColors.HasValue && string.IsNullOrEmpty(model.Color))
            {
                this.Response.StatusCode = 203;
                return Content("Please provide value for color.");
            }
            if ((model.AllStyles.HasValue && string.IsNullOrEmpty(model.Style)) && string.IsNullOrEmpty(model.SewingPlantId) && model.AllColors.HasValue && string.IsNullOrEmpty(model.Color))
            {
                this.Response.StatusCode = 203;
                return Content("You can not select 'All' for all three i.e Sewing Plant,Style and Color. Please provide value of at least one.");
            }
            //var errors = new List<string>();
            //if (!model.AllStyles.HasValue && string.IsNullOrEmpty(model.Style))
            //{
            //    errors.Add("Please provide value for style.");
            //}
            //if (!model.AllColors.HasValue && string.IsNullOrEmpty(model.Color))
            //{

            //    errors.Add("Please provide value for color.");
            //}
            //if (model.SpotCheckPercent == null)
            //{
            //    errors.Add("Please provide value for spotcheck %");

            //}

            //if ((model.AllStyles.HasValue && string.IsNullOrEmpty(model.Style)) && string.IsNullOrEmpty(model.SewingPlantId) && model.AllColors.HasValue && string.IsNullOrEmpty(model.Color))
            //{
            //    errors.Add("You can not select 'All' for all three i.e Sewing Plant,Style and Color. Please provide value of at least one.");
            //}
            //if (errors.Count > 0)
            //{
            //    this.Response.StatusCode = 203;               
            //    return Content(string.Join(";", errors));
            //}

            try
            {
                //Extra safety check.
                // Ignore value of style autocomplete in case All style check box is checked.Same for color.
                if (model.AllStyles.HasValue && model.AllStyles.Value)
                {
                    model.Style = null;
                }
               
                if (model.AllColors.HasValue && model.AllColors.Value)
                {
                    model.Color = null;
                }
               
                _service.SetSpotCheckPercentage(Map(model));
                var sd = _service.GetSpotCheckList();
                var list = sd.Select(p => Map(p)).ToList();
                ViewBag.key = model.ConfigurationKey;
                ViewBag.EnableEditing = AuthorizeExAttribute.IsSuperUser(HttpContext) || this.HttpContext.User.IsInRole(ROLE_RAD_EDITING);
                return PartialView(Views._spotCheckListPartial, list);

            }
            catch (Exception ex)
            {
                // Simulate the behavior of the obsolete HandleAjaxError attribute
                this.Response.StatusCode = 203;
                return Content(ex.Message);
            }
        }

        [HttpPost]
        [AuthorizeEx("Updating Receiving Configuration {0}", Roles = ROLE_RAD_EDITING)]
        public virtual ActionResult DeleteSpotCheckPercentage(SpotCheckViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ValidationErrorResult();
            }
            try
            {
                var dto = Map(model);
                dto.SpotCheckPercent = null;
                _service.SetSpotCheckPercentage(dto);
                var sd = _service.GetSpotCheckList();
                var list = sd.Select(p => Map(p)).ToList();
                ViewBag.EnableEditing = AuthorizeExAttribute.IsSuperUser(HttpContext) || this.HttpContext.User.IsInRole(ROLE_RAD_EDITING);
                return PartialView(Views._spotCheckListPartial, list);
            }
            catch (Exception ex)
            {
                // Simulate the behavior of the obsolete HandleAjaxError attribute
                this.Response.StatusCode = 203;
                return Content(ex.Message);
            }
        }

        //protected override ViewResult View(string viewName, string masterName, object model)
        //{
        //    // All view models must be derived from ViewModelBase
        //    var vmb = (ViewModelBase)model;
        //    vmb.QueryCount = _service.QueryCount;
        //    return base.View(viewName, masterName, model);
        //}
    }
}




//$Id$