using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using DcmsMobile.Inquiry.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.Inquiry.Areas.Inquiry.IntransitEntity
{
    [RoutePrefix("itr")]
    public partial class IntransitEntityController : InquiryControllerBase
    {
        #region Intialization

        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public IntransitEntityController()
        {

        }

        private IntransitEntityService _service;

        internal IntransitEntityService Service
        {

            get { return _service; }

            set { _service = value; }
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (_service == null)
            {
                var connectString = ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString;
                var userName = requestContext.HttpContext.SkipAuthorization ? string.Empty : requestContext.HttpContext.User.Identity.Name;
                var clientInfo = string.IsNullOrEmpty(requestContext.HttpContext.Request.UserHostName) ? requestContext.HttpContext.Request.UserHostAddress :
                    requestContext.HttpContext.Request.UserHostName;
                _service = new IntransitEntityService(requestContext.HttpContext.Trace, connectString, userName, clientInfo);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_service != null)
            {
                _service.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Shipment Lists
        [Route("excel")]
        public virtual ActionResult IntransitShipmentListExcel(ShipmentListFilterModel filters)
        {
            string heading;
            switch (filters.Source)
            {
                case null:
                    heading = "All Shipments";
                    break;

                case ShipmentSourceType.Vendor:
                    heading = "Vendor Shipments";
                    break;

                case ShipmentSourceType.Transfer:
                    heading = "Transfer Shipments";
                    break;

                default:
                    throw new NotImplementedException();
            }
            var list = GetShipmentModels(filters, Helpers.GlobalConstants.MAX_EXCEL_ROWS);

            var result = new ExcelResult("Inbound Shipment Summary");           
            //TODO: include filters in heading
            result.AddWorkSheet(list, "Shipments", heading);
            return result;
        }

        private IList<IntransitShipmentModel> GetShipmentModels(ShipmentListFilterModel filters, int maxRows)
        {
            ShipmentFilters statusFilter = ShipmentFilters.NoFilter;
            if (filters.Status == ShipmentStatusType.Open)
            {
                // Showing open shipments.
                statusFilter |= ShipmentFilters.OpenShipments;
                filters.MinClosedDate = null;
                filters.MaxClosedDate = null;
            }
            else
            {
                // Showing close shipments.
                statusFilter |= ShipmentFilters.ClosedShipments;              
            }
            if (filters.VariancesOnly)
            {
                statusFilter |= ShipmentFilters.VarianceOnlyShipments;
            }

            switch (filters.Source)
            {
                case null:
                    break;

                case ShipmentSourceType.Vendor:
                    statusFilter |= ShipmentFilters.VendorShipments;
                    break;

                case ShipmentSourceType.Transfer:
                    statusFilter |= ShipmentFilters.BuildingTransferShipments;
                    break;

                default:
                    throw new NotImplementedException();
            }
            // int nMaxRowsToShow = int.MaxValue;
            var shipmentList = _service.GetInboundShipmentSummary(filters.MinClosedDate, filters.MaxClosedDate, statusFilter, filters.SewingPlantCode, maxRows);

            var list = from row in shipmentList
                       orderby (row.IsShipmentClosed ? row.MaxUploadDate : row.ShipmentDate) descending
                       select new IntransitShipmentModel(row);

            return list.ToList();
        }
        /// <summary>
        /// Displays closed shipments by default.
        /// </summary>
        /// <param name="model">
        /// IsOpenShipments,IncludeBuildingTransfers,IncludeVendorShipments,VarianceShipments,MinClosedDate,MaxClosedDate
        /// </param>
        /// <returns></returns>    
        [Route("list", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_IntransitShipmentList)]
        public virtual ActionResult IntransitShipmentList(ShipmentListFilterModel filters)
        {
            //We want to show top 500 rows.
            int nMaxRowsToShow = 500;
            var shipmentLists = GetShipmentModels(filters, nMaxRowsToShow);
            var model = new IntransitShipmentListViewModel
            {
                Filters = filters
            };
            //model.TotalShipmentRows = shipmentLists.Select(m => m.TotalShipmentCount).FirstOrDefault();
            //model.MaxRowsToShow = nMaxRowsToShow;
            model.Shipments = shipmentLists;
            model.SewingPlantList = (from item in _service.GetSewingPlantList()
                                     select new SelectListItem
                                     {
                                         Text = string.Format("{0}:{1}", item.SewingPlantCode, item.PlantName),
                                         Value = item.SewingPlantCode
                                     }).ToList();
            return View(Views.IntransitShipmentList, model);
        }

        #endregion




        private IList<ShipmentSkuGroup> GetInboundShipmentDetailModels(ShipmentSkuFilterModel filters, int maxRows)
        {
            ShipmentSkuFilters statusFilter = ShipmentSkuFilters.NoFilter;
            if (filters.SkuStatus == ShipmentSkuStatusType.Varience)
            {
                statusFilter |= ShipmentSkuFilters.VarianceSku;

            }
            switch (filters.SkuSource)
            {
                case null:
                    break;

                case ShipmentSkuSourceType.Vendor:
                    statusFilter |= ShipmentSkuFilters.VendorShipments;
                    break;

                case ShipmentSkuSourceType.Transfer:
                    statusFilter |= ShipmentSkuFilters.BuildingTransferShipments;
                    break;

                default:
                    throw new NotImplementedException();
            }


            var shipmentList = _service.GetInboundShipmentSkuDetail(statusFilter, filters.MaxClosedDate, filters.MinClosedDate, filters.SewingPlantCode, maxRows);
            var list = from shipment in shipmentList
                       group shipment by new
                       {
                           InstransitType = string.IsNullOrWhiteSpace(shipment.IntransitType) || shipment.IntransitType == "IT" ? "" : shipment.IntransitType == "ZEL" || shipment.IntransitType == "TR" ? "ZEL" : "Unknown"
                       }
                           into g
                           select new ShipmentSkuGroup
                           {
                               InstransitType = g.Key.InstransitType,
                               Shipments = (from p in g
                                            orderby p.UploadDate descending, p.ShipmentId, p.Style, p.Color, p.Dimension, p.SkuSize
                                            select new ShipmentDetailSkuModel(p)).ToList()
                           };


            return list.ToList();
        }

        /// <summary>
        /// Displays list of closed shipments per sku.
        /// </summary>        
        [Route("skulist")]
        public virtual ActionResult InboundShipmentDetail(ShipmentSkuFilterModel filters, bool exportExcel = false)
        {

            int nMaxRowsToShow = 2000;
            var shipmentLists = GetInboundShipmentDetailModels(filters, nMaxRowsToShow);
            var model = new IntransitShipmentSkuListViewModel
            {
                Filters = filters
            };
            model.ShipmentLists = shipmentLists;
            //model.TotalShipmentRows = shipmentLists.Select(m => m.Shipments[0].TotalShipmentCount).FirstOrDefault();
         
            model.MaxRowsToShow = shipmentLists.Select(m => m.Shipments).Distinct().Count();

            model.SewingPlantList = (from item in _service.GetSewingPlantList()
                                     select new SelectListItem
                                     {
                                         Text = string.Format("{0}:{1}", item.SewingPlantCode, item.PlantName),
                                         Value = item.SewingPlantCode
                                     }).ToArray();

            if (exportExcel)
            {
                var result = new ExcelResult("Shipment_SKU_Detail");
                result.AddWorkSheet(model.TransferShipmentList, "Transfers", "List of Building Transfer Shipment");
                result.AddWorkSheet(model.UnknownShipmentList, "Unknown", "List of Unknown Shipment");
                result.AddWorkSheet(model.VendorShipmentList, "Vendors", "List of Vendor Shipment");
                return result;
            }
            return View(Views.IntransitShipmentSkuList, model);
        }





        [Route("shipment/{id}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchIntransitShipment1)]
        [SearchQuery(@"Select {0}, S.Original_Shipment_Id, 'Intransit Shipment ' || S.Original_Shipment_Id, NULL, NULL FROM <proxy />SRC_CARTON_INTRANSIT S WHERE S.Original_Shipment_Id = :search_text AND rownum &lt; 2")]
        public virtual ActionResult IntransitShipment(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var shipment = _service.GetInboundShipmentInfo(id);
            if (shipment == null || shipment.Count == 0)
            {
                AddStatusMessage(string.Format("No info found for {0} ", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var shipmentHeader = shipment.First();
            var model = new IntransitShipmentViewModel
            {
                CreatedOn = shipment.Max(P => P.CreatedOn),
                IntransitId = shipmentHeader.IntransitId,
                MaxReceiveDate = shipment.Max(p => p.MaxReceiveDate),
                MinReceiveDate = shipment.Max(p => p.MinReceiveDate),
                SewingPlantCode = shipmentHeader.SewingPlantCode,
                SewingPlantName = shipmentHeader.SewingPlantName,
                ShipmentDate = shipment.Max(P => P.ShipmentDate),
                ShipmentId = id,
                VwhCount = shipment.Select(p => p.Vwh).Distinct().Count(),
                MaxVwh = shipment.Max(p => p.Vwh),
                MinVwh = shipment.Min(p => p.Vwh),
                IsShipmentClosed = string.IsNullOrWhiteSpace(shipmentHeader.IsShipmentClosed) ? "Open" : "Closed",
                IntransitType = shipmentHeader.IntransitType,
                ErpId = shipmentHeader.ErpId,
                ShipmentSku = shipment.Select(p => new ShipmentSkuModel
                {
                    Style = p.Style,
                    Color = p.Color,
                    Dimension = p.Dimension,
                    SkuSize = p.SkuSize,
                    ReceivedPieces = p.ReceivedPieces ?? 0,
                    ExpectedPieces = p.ExpectedPieces ?? 0,
                    UnderReceviedPieces = p.UnderReceviedPieces ?? 0,
                    OverReceviedPieces = p.OverReceviedPieces ?? 0,
                    ExpectedCartonCount = p.ExpectedCartonCount,
                    ReceivedCartonCount = p.ReceivedCartonCount,
                    UnderReceviedCartonCount = p.UnderReceviedCartonCount,
                    OverReceviedCartonCount = p.OverReceviedCartonCount,
                    MinMergedToBuddyShipment = p.MinMergedToBuddyShipment,
                    MaxMergedToBuddyShipment = p.MaxMergedToBuddyShipment,
                    MinMergedInBuddyShipment = p.MinMergedInBuddyShipment,
                    MaxMergedInBuddyShipment = p.MaxMergedInBuddyShipment,
                    CountMergedToBuddyShipment = p.CountMergedToBuddyShipment,
                    CountMergedInBuddyShipment = p.CountMergedInBuddyShipment,
                    CtnsReceivedInOtherShipment = p.CtnsReceivedInOtherShipment,
                    PcsReceivedInOtherShipment = p.PcsReceivedInOtherShipment
                }).OrderByDescending(m => m.ExpectedPieces - m.ReceivedPieces).ToList()
            };

            var route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Receving];
            if (route != null)
            {
                model.UrlReceiveCarton = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Receving);
            }
            //model.DcmsLinks.Add(new DcmsLinkModel
            //{
            //    ShortDescription = "Receive Cartons",
            //    Url = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Receving)
            //});

            return View(Views.IntransitShipment, model);
        }


        [Route("shipment/excel/{id}")]
        public virtual ActionResult IntransitShipmentExcel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            var shipmentSku = _service.GetInboundShipmentInfo(id).Select(p => new ShipmentSkuModel
            {
                Style = p.Style,
                Color = p.Color,
                Dimension = p.Dimension,
                SkuSize = p.SkuSize,
                ReceivedPieces = p.ReceivedPieces,
                ExpectedPieces = p.ExpectedPieces,
                UnderReceviedPieces = p.UnderReceviedPieces,
                OverReceviedPieces = p.OverReceviedPieces,
                ExpectedCartonCount = p.ExpectedCartonCount,
                ReceivedCartonCount = p.ReceivedCartonCount,
                UnderReceviedCartonCount = p.UnderReceviedCartonCount,
                OverReceviedCartonCount = p.OverReceviedCartonCount,
                MinMergedToBuddyShipment = p.MinMergedToBuddyShipment,
                MaxMergedToBuddyShipment = p.MaxMergedToBuddyShipment,
                MinMergedInBuddyShipment = p.MinMergedInBuddyShipment,
                MaxMergedInBuddyShipment = p.MaxMergedInBuddyShipment,
                CountMergedToBuddyShipment = p.CountMergedToBuddyShipment,
                CountMergedInBuddyShipment = p.CountMergedInBuddyShipment,
                CtnsReceivedInOtherShipment = p.CtnsReceivedInOtherShipment,
                PcsReceivedInOtherShipment = p.PcsReceivedInOtherShipment
            }).OrderByDescending(m => m.ExpectedPieces - m.ReceivedPieces).ToList();

            var result = new ExcelResult("IntransitShipment_" + id);
            result.AddWorkSheet(shipmentSku, "SKU", "List of SKU in shipment " + id);
            return result;
        }


    }
}
