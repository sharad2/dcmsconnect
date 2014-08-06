using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.Repack.Models;
using DcmsMobile.Repack.Repository;
using DcmsMobile.Repack.ViewModels;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.Repack.Areas.Repack.Controllers
{
    /// <summary>
    /// Security:
    /// Each Action is decorated with the role required. Therefore unauthenticated users will not be able to access the Repack application.
    /// The Index Action does not require a role, but every UI chosen will require a specific role.
    /// </summary>
    [RouteArea("Repack")]
    [RoutePrefix(HomeController.NameConst)]
    public partial class HomeController : EclipseController
    {
        #region Initialize
        public HomeController()
        {

        }

        private RepackService _service;
        protected override void Initialize(RequestContext requestContext)
        {
            _service = new RepackService(requestContext);
            base.Initialize(requestContext);
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        /// Redirect to Repack if cookie is found
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Before redirecting we ensure that the user has rights to access the UI
        /// </remarks>
        [ActionName("Index")]
        [Route(HomeController.ActionNameConstants.Index, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Repack)]
        public virtual ActionResult Index()
        {
            var cookie = this.HttpContext.Request.Cookies[RepackViewModel.KEY_UISTYLE];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                var uiStyle = (RepackUiStyle)Enum.Parse(typeof(RepackUiStyle), cookie.Value);
                RedirectToRouteResult rr;
                switch (uiStyle)
                {
                    case RepackUiStyle.Storage:
                        rr = RedirectToAction(MVC_Repack.Repack.Home.RepackStorage());
                        break;

                    case RepackUiStyle.Conversion:
                        rr = RedirectToAction(MVC_Repack.Repack.Home.RepackConversion());
                        break;

                    case RepackUiStyle.BulkConversion:
                        rr = RedirectToAction(MVC_Repack.Repack.Home.RepackBulkConversion());
                        break;

                    case RepackUiStyle.Receive:
                        rr = RedirectToAction(MVC_Repack.Repack.Home.RepackReceive());
                        break;

                    default:
                        // Deliberately ignoring cookie for the advanced UI.
                        cookie.Expires = DateTime.Now.AddDays(-1);
                        this.HttpContext.Response.Cookies.Add(cookie);
                        rr = null;
                        break;
                }
                if (rr != null)
                {
                    var action = (string)rr.RouteValues["Action"];
                    var roleAttr = (AuthorizeExAttribute)this.GetType().GetMember(action).First()
                        .GetCustomAttributes(typeof(AuthorizeExAttribute), true).FirstOrDefault();
                    // Assume that a single role has been specified
                    if (roleAttr == null || string.IsNullOrWhiteSpace(roleAttr.Roles) || this.HttpContext.User.IsInRole(roleAttr.Roles))
                    {
                        // Since the user is authorized for the UI, go ahead and redirect
                        return rr;
                    }
                }
            }
            return View();
        }

        /// <summary>
        /// Expire the cookie before invoking the Ui page
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult ChangeUi()
        {
            var cookie = this.HttpContext.Request.Cookies[RepackViewModel.KEY_UISTYLE];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                this.HttpContext.Response.Cookies.Add(cookie);
            }
            return RedirectToAction(MVC_Repack.Repack.Home.Index());
        }


        private IDictionary<string, IEnumerable<SelectListItem>> MapInventoryArea(IDictionary<string, IEnumerable<InventoryArea>> entity)
        {
            var inventoryAreaList = (from a in entity
                                     select new
                                     {
                                         Keys = a.Key,
                                         Values = a.Value.Select(p => new SelectListItem
                                         {
                                             Text = p.ShortName + ": " + p.Description,
                                             Value = p.IaId
                                         })
                                     }).ToDictionary(p => p.Keys, p => p.Values);

            return inventoryAreaList;

        }

        private IDictionary<string, IEnumerable<SelectListItem>> MapQuality(IDictionary<string, IList<Quality>> entity)
        {
            var qualityList = (from a in entity
                               select new
                               {
                                   Keys = a.Key,
                                   Values = a.Value.Select(p => new SelectListItem
                                   {
                                       Text = p.QualityCode + ": " + p.Description,
                                       Value = p.QualityCode
                                   })
                               }).ToDictionary(p => p.Keys, p => p.Values);

            return qualityList;

        }

        private IDictionary<string, IEnumerable<SelectListItem>> MapVWH(IDictionary<string, IList<VirtualWarehouse>> entity)
        {
            var vwhList = (from a in entity
                           select new
                           {
                               Keys = a.Key,
                               Values = a.Value.Select(p => new SelectListItem
                               {
                                   Text = p.VwhId + ": " + p.Description,
                                   Value = p.VwhId
                               })
                           }).ToDictionary(p => p.Keys, p => p.Values);

            return vwhList;

        }

        private IDictionary<string, IEnumerable<SelectListItem>> MapPriceSeason(IDictionary<string, IList<PriceSeason>> entity)
        {
            var priceSeasonList = (from a in entity
                                   select new
                                   {
                                       Keys = a.Key,
                                       Values = a.Value.Select(p => new SelectListItem
                                       {
                                           Text = p.PriceSeasonCode + ": " + p.Description,
                                           Value = p.PriceSeasonCode
                                       })
                                   }).ToDictionary(p => p.Keys, p => p.Values);

            return priceSeasonList;
        }

        private IDictionary<string, IEnumerable<SelectListItem>> MapPrinter(IDictionary<string, IList<Printer>> entity)
        {
            var printerList = (from a in entity
                               select new
                               {
                                   Keys = a.Key,
                                   Values = a.Value.Select(p => new SelectListItem
                                   {
                                       Text = p.PrinterName + ": " + p.Description,
                                       Value = p.PrinterName
                                   })
                               }).ToDictionary(p => p.Keys, p => p.Values);

            return printerList;
        }

        private SelectListItem MapSewinPalnt(SewingPlant entity)
        {
            return new SelectListItem
            {
                Text = entity.SewingPlantCode + ": " + entity.PlantName,
                Value = entity.SewingPlantCode
            };
        }

        #region Repack UI actions
        /// <summary>
        /// Sharad 19 Dec 2011: Advanced UI requires src_rpk privilege. 
        /// The role src_rpk must be assigned to anyone who is granted this privilege.
        /// </summary>
        /// <returns></returns>
        [AuthorizeEx("Advanced Repack requires Role {0}", Roles = "src_rpk")]
        public virtual ActionResult RepackAdvanced()
        {
            var model = new RepackViewModel(RepackUiStyle.Advanced)
            {
                PageHeading = "Advanced Create Cartons",
                SourceArea_List = MapInventoryArea(_service.GetGroupedAreas(InventoryAreaFilters.StoresSku | InventoryAreaFilters.GroupByUsability | InventoryAreaFilters.IncludeConsolidatedUpcAreas | InventoryAreaFilters.CancelledArea)),

                DestinationArea_List = MapInventoryArea(_service.GetGroupedAreas(InventoryAreaFilters.StoresCarton | InventoryAreaFilters.Unnumbered | InventoryAreaFilters.GroupByPalletRequirement)),

                QualityCode_List = MapQuality(_service.GetQualities(QualityType.All)),

                TargetVwhId_List = MapVWH(_service.GetVirtualWarehouses()),

                TargetQualityCode_List = MapQuality(_service.GetQualities(QualityType.QualityOrderByDesc)),

                PriceSeasonCode_List = MapPriceSeason(_service.GetPriceSeasonCodes()),

                SewingPlantCode_List = _service.GetGroupedSewingPlants().Select(p => MapSewinPalnt(p)),

                VwhId_List = MapVWH(_service.GetVirtualWarehouses()),

                PrinterName_List = MapPrinter(_service.GetZebraPrinters())
            };
            return DoRepack(model);
        }

        /// <summary>
        /// Sharad 19 Dec 2011: Advanced UI requires src_rpk privilege. The role src_rpk must be assigned to anyone who is granted this privilege.
        /// </summary>
        /// <returns></returns>
        [AuthorizeEx("Bulk Repack Advanced requires Role {0}", Roles = "src_rpk")]
        public virtual ActionResult RepackBulkAdvanced()
        {
            var model = new RepackViewModel(RepackUiStyle.BulkAdvanced)
            {
                PageHeading = "Advanced Bulk Create Cartons",

                SourceArea_List = MapInventoryArea(_service.GetGroupedAreas(InventoryAreaFilters.StoresSku | InventoryAreaFilters.GroupByUsability | InventoryAreaFilters.IncludeConsolidatedUpcAreas)),

                DestinationArea_List = MapInventoryArea(_service.GetGroupedAreas(InventoryAreaFilters.StoresCarton | InventoryAreaFilters.Unnumbered | InventoryAreaFilters.GroupByPalletRequirement)),

                QualityCode_List = MapQuality(_service.GetQualities(QualityType.All)),

                TargetVwhId_List = MapVWH(_service.GetVirtualWarehouses()),

                TargetQualityCode_List = MapQuality(_service.GetQualities(QualityType.QualityOrderByDesc)),

                PriceSeasonCode_List = MapPriceSeason(_service.GetPriceSeasonCodes()),

                SewingPlantCode_List = _service.GetGroupedSewingPlants().Select(MapSewinPalnt),

                VwhId_List = MapVWH(_service.GetVirtualWarehouses()),

                PrinterName_List = MapPrinter(_service.GetZebraPrinters())
            };
            return DoRepack(model);
        }

        [AuthorizeEx("Repack for Recieve requires Role {0}", Roles = "src_rpk")]
        public virtual ActionResult RepackReceive()
        {
            var model = new RepackViewModel(RepackUiStyle.Receive)
            {
                PageHeading = "Blind Receiving",
                DestinationArea_List = MapInventoryArea(_service.GetGroupedAreas(InventoryAreaFilters.ReceivingAreas | InventoryAreaFilters.Usable | InventoryAreaFilters.GroupByPalletRequirement)),

                QualityCode = _service.GetQuality(QualityType.Received).QualityCode,

                PriceSeasonCode_List = MapPriceSeason(_service.GetPriceSeasonCodes()),

                SewingPlantCode_List = _service.GetGroupedSewingPlants().Select(MapSewinPalnt),

                VwhId_List = MapVWH(_service.GetVirtualWarehouses()),

                PrinterName_List = MapPrinter(_service.GetZebraPrinters()),
                AllowCartonScan = true
            };
            return DoRepack(model);
        }

        [AuthorizeEx("Bulk Repack for conversion requires Role {0}", Roles = "src_rpk")]
        public virtual ActionResult RepackBulkConversion()
        {
            var model = new RepackViewModel(RepackUiStyle.BulkConversion)
            {
                PageHeading = "Bulk Repack for Conversion",
                SourceArea_List = MapInventoryArea(_service.GetGroupedAreas(InventoryAreaFilters.Usable | InventoryAreaFilters.StoresSku | InventoryAreaFilters.RepackForStorage)),

                DestinationArea_List = MapInventoryArea(_service.GetGroupedAreas(InventoryAreaFilters.StoresCarton | InventoryAreaFilters.Unnumbered | InventoryAreaFilters.ConversionAreasOnly | InventoryAreaFilters.GroupByPalletRequirement)),

                QualityCode = _service.GetQuality(QualityType.Order).QualityCode,

                PriceSeasonCode_List = MapPriceSeason(_service.GetPriceSeasonCodes()),

                SewingPlantCode_List = _service.GetGroupedSewingPlants().Select(MapSewinPalnt),

                TargetVwhId_List = MapVWH(_service.GetVirtualWarehouses()),

                TargetQualityCode_List = MapQuality(_service.GetQualities(QualityType.QualityOrderByDesc)),

                VwhId_List = MapVWH(_service.GetVirtualWarehouses()),

                PrinterName_List = MapPrinter(_service.GetZebraPrinters()),
                ConvertSku = true
            };
            return DoRepack(model);
        }

        [AuthorizeEx("Repack for conversion requires Role {0}", Roles = "src_rpk")]
        public virtual ActionResult RepackConversion()
        {
            var model = new RepackViewModel(RepackUiStyle.Conversion)
            {
                PageHeading = "Repack for Conversion",
                SourceArea_List = MapInventoryArea(_service.GetGroupedAreas(InventoryAreaFilters.Usable | InventoryAreaFilters.StoresSku | InventoryAreaFilters.RepackForStorage)),

                DestinationArea_List = MapInventoryArea(_service.GetGroupedAreas(InventoryAreaFilters.StoresCarton | InventoryAreaFilters.Unnumbered | InventoryAreaFilters.ConversionAreasOnly | InventoryAreaFilters.GroupByPalletRequirement)),

                QualityCode = _service.GetQuality(QualityType.Order).QualityCode,

                PriceSeasonCode_List = MapPriceSeason(_service.GetPriceSeasonCodes()),

                SewingPlantCode_List = _service.GetGroupedSewingPlants().Select(MapSewinPalnt),

                TargetVwhId_List = MapVWH(_service.GetVirtualWarehouses()),

                VwhId_List = MapVWH(_service.GetVirtualWarehouses()),

                TargetQualityCode_List = MapQuality(_service.GetQualities(QualityType.QualityOrderByDesc)),

                PrinterName_List = MapPrinter(_service.GetZebraPrinters()),
                ConvertSku = true
            };
            return DoRepack(model);
        }

        [AuthorizeEx("Repack from Shelf Stock or SSS requires Role {0}", Roles = "src_rpk")]
        public virtual ActionResult RepackStorage()
        {
            var model = new RepackViewModel(RepackUiStyle.Storage)
            {
                PageHeading = "Repack from Shelf Stock or SSS",
                SourceArea_List = MapInventoryArea(_service.GetGroupedAreas(InventoryAreaFilters.Usable | InventoryAreaFilters.StoresSku | InventoryAreaFilters.RepackForStorage | InventoryAreaFilters.IncludeConsolidatedUpcAreas | InventoryAreaFilters.Unnumbered)),

                DestinationArea_List = MapInventoryArea(_service.GetGroupedAreas(InventoryAreaFilters.StoresCarton | InventoryAreaFilters.Unnumbered | InventoryAreaFilters.ExcludeConversionAreas  | InventoryAreaFilters.GroupByPalletRequirement)),

                QualityCode = _service.GetQuality(QualityType.Order).QualityCode,

                PriceSeasonCode_List = MapPriceSeason(_service.GetPriceSeasonCodes()),

                SewingPlantCode_List = _service.GetGroupedSewingPlants().Select(MapSewinPalnt),

                VwhId_List = MapVWH(_service.GetVirtualWarehouses()),

                PrinterName_List = MapPrinter(_service.GetZebraPrinters())
            };
            return DoRepack(model);
        }

        /// <summary>
        /// Save the UI style in a cookie for future use
        /// </summary>
        /// <param name="model"> </param>
        /// <returns></returns>
        [NonAction]
        private ActionResult DoRepack(RepackViewModel model)
        {
            var cookie = new HttpCookie(RepackViewModel.KEY_UISTYLE, model.UiStyle.ToString())
                {
                    Expires = DateTime.Now.AddDays(3)
                };
            this.HttpContext.Response.Cookies.Add(cookie);
            return View(this.Views.Repack, model);
        }
        #endregion

        /// <summary>
        /// This will be called via AJAX. The passed information contains everything you need to know to create a new carton.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult RepackCarton(RepackViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView(MVC_Repack.Repack.Shared.Views._validationPartial);
            }
            //TC 9: When ModelState is valid

            var info = new CartonRepackInfo
            {
                CartonId = model.CartonId != null ? model.CartonId.ToUpper() : null,
                DestinationCartonArea = model.DestinationArea,
                SourceSkuArea = model.SourceArea,
                VwhId = model.VwhId,
                PalletId = model.PalletId != null ? model.PalletId.ToUpper() : model.PalletId,
                ShipmentId = model.ShipmentId,
                PriceSeasonCode = model.PriceSeasonCode,
                QualityCode = model.QualityCode,
                SewingPlantCode = model.SewingPlantCode,
                Pieces = model.Pieces.Value,
                NumberOfCartons = model.NumberOfCartons.Value,
                PrinterName = model.PrinterName,
                TargetVWhId = model.ConvertSku ? model.TargetVwhId : null,
                TargetQualityCode = model.ConvertSku ? model.TargetQualityCode : null
            };
            try
            {
                var sourceSku = _service.GetSkuFromBarCode(model.SkuBarCode);
                string msg;
                if (sourceSku == null)
                {
                    ModelState.AddModelError("",string.Format("{0} is invalid SKU",model.SkuBarCode));
                    return PartialView(MVC_Repack.Repack.Shared.Views._validationPartial);
                }
                if (model.ConvertSku)
                {
                    var targetSku = _service.GetSkuFromBarCode(model.TargetSkuBarCode);
                    if (targetSku == null)
                    {
                        ModelState.AddModelError("", string.Format("{0} is invalid Target SKU", model.TargetSkuBarCode));
                        return PartialView(MVC_Repack.Repack.Shared.Views._validationPartial);
                    }
                    info.TartgetSkuId = targetSku.SkuId;
                  //  info.UpcCode = targetSku.UpcCode;
                }

                info.SkuId = sourceSku.SkuId;
                info.UpcCode = sourceSku.UpcCode;
                var cartons = _service.RepackCarton(info);

                //TC 10: When user scan invalid carton
                if (cartons == null)
                {
                    ModelState.AddModelError("", "Could not create cartons");
                    return PartialView(MVC_Repack.Repack.Shared.Views._validationPartial);
                }

                //TC 11: When user chose a valid printer.
                if (!string.IsNullOrEmpty(model.PrinterName))
                {
                    AddStatusMessage(string.Format("Carton ticket printed on {0}", model.PrinterName));
                }
                //TC 12: When single carton created.
                if (model.NumberOfCartons == 1)
                {
                    msg = string.Format("Carton {0} created", cartons[0]);
                }
                //TC 13: When multiple cartons created.
                else
                {
                    msg = string.Format("{1} Cartons created. First carton id {0} ,Last carton id {2}",
                        cartons[0], model.NumberOfCartons, cartons[1]);
                }
                AddStatusMessage(msg);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return PartialView(MVC_Repack.Repack.Shared.Views._validationPartial);
        }

        public virtual ActionResult Tutorial()
        {
            return View(Views.Tutorial);
        }
    }
}




//$Id$