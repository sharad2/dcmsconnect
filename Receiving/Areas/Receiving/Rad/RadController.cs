﻿using DcmsMobile.Receiving.Areas.Receiving.Rad;
using DcmsMobile.Receiving.Models.Rad;
using DcmsMobile.Receiving.Repository;
using DcmsMobile.Receiving.ViewModels.Rad;
using EclipseLibrary.Mvc.Controllers;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;


namespace DcmsMobile.Receiving.Areas.Receiving.Controllers
{
    public partial class RadController : EclipseController
    {
        private const string ROLE_RAD_EDITING = "SRC_RECEIVING_MGR";

        private SelectListItem Map(SewingPlant src)
        {
            return new SelectListItem
            {
                Text = src.SewingPlantCode + ": " + src.PlantName,
                Value = src.SewingPlantCode
                //GroupText = src.GroupingColumn + ":" + src.CountryName
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
            var model = new IndexViewModel();
            var sc = _service.GetSpotCheckList();
            model.SpotCheckList = sc.Select(p =>new SpotCheckConfigurationModel(p)).ToList();

            model.EnableEditing = AuthorizeExAttribute.IsSuperUser(HttpContext) || this.HttpContext.User.IsInRole(ROLE_RAD_EDITING);
            model.SpotCheckAreaList = _service.GetSpotCheckAreas().Select(p => new SpotCheckAreaModel(p)).ToList();
            ViewBag.EnableEditing = model.EnableEditing;

            return View(Views.Index, model);
        }

        /// <summary>
        /// Returns the partial view for adding spot check setting
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult AddSpotCheckPartial()
        {
            var plantlist = _service.GetSewingPlants().Select(p => Map(p));
            var model = new AddSpotCheckViewModel
            {
                SewingPlantList = plantlist
            };
            return PartialView(Views._addSpotCheckPartial, model);
        }



        [HttpPost]
        [AuthorizeEx("Updating Receiving Configuration {0}", Roles = ROLE_RAD_EDITING)]
        public virtual ActionResult AddUpdateSpotCheckSetting(ModifyAction action, string style, string color, string sewingPlantId, int? spotCheckPercent, bool enabled)
        {

            if (action == ModifyAction.Delete)
            {
                _service.DeleteSpotCheckSetting(style, sewingPlantId);
            }
            else 
            {
                _service.AddUpdateSpotCheckSetting(style, color, sewingPlantId, spotCheckPercent, enabled);
            }
            return RedirectToAction(MVC_Receiving.Receiving.Rad.Index());
        
        }




      

    }
}




//$Id$