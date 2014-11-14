

using EclipseLibrary.Mvc.Controllers;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;


namespace DcmsMobile.Receiving.Areas.Receiving.Rad
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
          
            model.EnableEditing = AuthorizeExAttribute.IsSuperUser(HttpContext) || this.HttpContext.User.IsInRole(ROLE_RAD_EDITING);
            model.SpotCheckAreaList = _service.GetSpotCheckAreas().Select(p => new SpotCheckAreaModel(p)).ToList();
           

            var query = from item in sc                       
                        group item by
                            item.SewingPlantId into g
                        let defstyle = g.Where(p => string.IsNullOrWhiteSpace(p.Style)).FirstOrDefault()
                        orderby g.Key
                        select new SewingPlantGroupModel
                        {
                            SewingPlantId = g.Key,
                            PlantName = g.Where(p => p.SewingPlantId == g.Key).First().PlantName,
                            SpotCheckPercent = defstyle == null ? null : defstyle.SpotCheckPercent,
                            CreatedBy = defstyle == null ? "" : defstyle.CreatedBy,
                            CreatedDate = defstyle == null ? null : defstyle.CreatedDate,
                            IsSpotCheckEnabled = defstyle == null ? true : (defstyle.IsSpotCheckEnable ?? true),
                            ModifiedBy = defstyle == null ? "" : defstyle.ModifiedBy,
                            ModifiedDate = defstyle == null ? null : defstyle.ModifiedDate,
                            Styles = (from item2 in g
                                      where !string.IsNullOrWhiteSpace(item2.Style)
                                      orderby item2.Style, item2.Color
                                      select new SpotCheckConfigurationModel(item2)).ToList()

                        };

            model.SewingPlants = query.ToList();

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
        public virtual ActionResult AddUpdateSpotCheckSetting(ModifyAction action, string style, string color, string sewingPlantId,
            int? spotCheckPercent, bool enabled = false)
        {

            if (action == ModifyAction.Delete)
            {
                var rows = _service.DeleteSpotCheckSetting(style,color, sewingPlantId);
                AddStatusMessage(string.Format("{0} Spot check setting has been deleted", rows));
            }
            else
            {
                var inserted = _service.AddUpdateSpotCheckSetting(style, color, sewingPlantId, spotCheckPercent, enabled);
                if (inserted)
                {
                    AddStatusMessage("Spot check setting has been added"); // TODO: for which style etc.
                }
                else
                {
                    AddStatusMessage("Spot check setting has been modified");
                }
            }
            return RedirectToAction(MVC_Receiving.Receiving.Rad.Index());

        }






    }
}




//$Id$