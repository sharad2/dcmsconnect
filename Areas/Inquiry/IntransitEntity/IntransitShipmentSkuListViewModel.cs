using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.Inquiry.Areas.Inquiry.IntransitEntity
{
    /// <summary>
    /// Where is this shipment coming from ?
    /// </summary>
    public enum ShipmentSkuSourceType
    {
        Vendor,
        Transfer
    }

    public enum ShipmentSkuStatusType
    {
        All,
        Varience,
        Dates
    }


    public class ShipmentSkuFilterModel
    {

        public ShipmentSkuFilterModel()
        {

        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other"></param>
        public ShipmentSkuFilterModel(ShipmentSkuFilterModel other)
        {
            SkuSource = other.SkuSource;
            SkuStatus = other.SkuStatus;
            SewingPlantCode = other.SewingPlantCode;
            MinClosedDate = other.MinClosedDate;
            MaxClosedDate = other.MaxClosedDate;
        }

        [DisplayFormat(NullDisplayText = "All Shipment Types")]
        public ShipmentSkuSourceType? SkuSource { get; set; }

        public ShipmentSkuStatusType SkuStatus { get; set; }

        public string SewingPlantCode { get; set; }

        /// <summary>
        /// This RegExp stolen from http://regexlib.com/REDetails.aspx?regexp_id=112
        /// </summary>
        /// <remarks>
        /// The following validates dates with and without leading zeros in the following formats: MM/DD/YYYY and it also takes YYYY (this can easily be removed).
        /// All months are validated for the correct number of days for that particular month except for February which can be set to 29 days. date day month year
        /// </remarks>
        //[RegularExpression(@"^((((0[13578])|([13578])|(1[02]))[\/](([1-9])|([0-2][0-9])|(3[01])))|(((0[469])|([469])|(11))[\/](([1-9])|([0-2][0-9])|(30)))|((2|02)[\/](([1-9])|([0-2][0-9]))))[\/]\d{4}$|^\d{4}$")]
        public DateTime? MinClosedDate { get; set; }

        //[RegularExpression(@"^((((0[13578])|([13578])|(1[02]))[\/](([1-9])|([0-2][0-9])|(3[01])))|(((0[469])|([469])|(11))[\/](([1-9])|([0-2][0-9])|(30)))|((2|02)[\/](([1-9])|([0-2][0-9]))))[\/]\d{4}$|^\d{4}$")]
        public DateTime? MaxClosedDate { get; set; }

        public string DisplayStatus
        {
            get
            {
                switch (SkuStatus)
                {
                    case ShipmentSkuStatusType.All:
                        return "All";
                    case ShipmentSkuStatusType.Varience:
                        return "Varience";
                    case ShipmentSkuStatusType.Dates:
                        if (MinClosedDate != null && MaxClosedDate != null)
                        {
                            return string.Format("From {0:d} To {1:d}", MinClosedDate, MaxClosedDate);
                        }
                        else if (MinClosedDate != null)
                        {
                            return string.Format("From {0:d}", MinClosedDate);
                        }
                        else
                        {
                            return string.Format("To {0:d}", MaxClosedDate);
                        }
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Returns true if any don default filter has been applied
        /// </summary>
        public bool HasFilters
        {
            get
            {
                return SkuSource != null || SkuStatus != ShipmentSkuStatusType.All || !string.IsNullOrWhiteSpace(SewingPlantCode)
                   || MinClosedDate != null || MaxClosedDate != null;
            }
        }

        public string DisplayFilters
        {
            get
            {
                List<string> list = new List<string>();
                if (this.SkuStatus != ShipmentSkuStatusType.All)
                {
                    list.Add(this.SkuStatus.ToString());
                }
                if (this.SkuSource.HasValue)
                {
                    list.Add(this.SkuSource.ToString());
                }
                if (this.MinClosedDate != null || this.MaxClosedDate != null)
                {
                    list.Add("Dates");
                }
                if (!string.IsNullOrWhiteSpace(this.SewingPlantCode))
                {
                    list.Add(string.Format("Sewing Plant {0}", SewingPlantCode));
                }
                return string.Join(", ", list);
            }
        }



    }
    public class ShipmentSkuGroup
    {
        /// <summary>
        /// Key is InstransitType
        /// </summary>
        public string InstransitType { get; set; }

        /// <summary>
        /// Display name of InstransitType
        /// </summary>
        public string DisplayInstransitType
        {
            get
            {
                return (string.IsNullOrEmpty(InstransitType) || this.InstransitType == "IT") ? "Vendor Shipments" : (InstransitType == "ZEL" || this.InstransitType == "TR") ? " Building Transfers" : "Unknown";
            }
        }

        /// <summary>
        /// List of shipments per sku
        /// </summary>
        public IList<ShipmentDetailSkuModel> Shipments { get; set; }




    }



    public class IntransitShipmentSkuListViewModel
    {

        #region Filter
        private ShipmentSkuFilterModel _filters;
        /// <summary>
        /// These filters will be posted
        /// </summary>
        public ShipmentSkuFilterModel Filters
        {
            get
            {
                if (_filters == null)
                {
                    _filters = new ShipmentSkuFilterModel();
                }
                return _filters;
            }
            set
            {
                _filters = value;
            }
        }


        #endregion
        public IList<SelectListItem> SewingPlantList { get; set; }


        /// <summary>
        /// Shipment list group by InstransitType.
        /// </summary>
        public IList<ShipmentSkuGroup> ShipmentLists { get; set; }
        

        // [AdditionalMetadata(ExcelAttribute.BUTTON_NAME, ExcelAttribute.BUTTON_NAME)]
        [Display(ShortName = "Vendors", Name = "Vendor Shipments", Order = 20)]
        public IList<ShipmentDetailSkuModel> VendorShipmentList
        {
            get
            {
                var vendorGroup = this.ShipmentLists.FirstOrDefault(p => string.IsNullOrWhiteSpace(p.InstransitType));
                if (vendorGroup == null)
                {
                    // There are no transfer shipments. Return null so that Excel will ignore it
                    return new List<ShipmentDetailSkuModel>();
                }
                return vendorGroup.Shipments;
            }
        }

        // [AdditionalMetadata(ExcelAttribute.BUTTON_NAME, ExcelAttribute.BUTTON_NAME)]
        [Display(ShortName = "Transfers", Name = "Building Transfer", Order = 10)]
        public IList<ShipmentDetailSkuModel> TransferShipmentList
        {
            get
            {
                var transferGroup = this.ShipmentLists.FirstOrDefault(p => p.InstransitType == "ZEL");
                if (transferGroup == null)
                {
                    // There are no transfer shipments. Return null so that Excel will ignore it
                    return new List<ShipmentDetailSkuModel>(); ;
                }
                return transferGroup.Shipments;
            }
        }


        //[AdditionalMetadata(ExcelAttribute.BUTTON_NAME, ExcelAttribute.BUTTON_NAME)]
        [Display(ShortName = "Unknown", Name = "Unknown", Order = 10)]
        public IList<ShipmentDetailSkuModel> UnknownShipmentList
        {
            get
            {
                var transferGroup = this.ShipmentLists.FirstOrDefault(p => p.InstransitType == "Unknown");
                if (transferGroup == null)
                {
                    // There are no transfer shipments. Return null so that Excel will ignore it
                    return new List<ShipmentDetailSkuModel>();
                }
                return transferGroup.Shipments;
            }
        }


        public string ExcelFileName
        {
            get { return "InboundShipmentSkuSummary"; }
        }

        public int? MaxRowsToShow { get; set; }

        //public object NameFor(Func<TModel, TProperty> func)
        //{
        //    throw new NotImplementedException();
        //}

        //public object NameFor(Func<TModel, TProperty> func)
        //{
        //    throw new NotImplementedException();
        //}
    }


    public class ShipmentDetailSkuModel
    {
        public ShipmentDetailSkuModel()
        {

        }

        internal ShipmentDetailSkuModel(IntransitShipmentSkuSummary p)
        {
            ShipmentId = p.ShipmentId;
            MinOtherShipmentId = p.MinOtherShipmentId;
            MaxOtherShipmentId = p.MaxOtherShipmentId;
            Style = p.Style;
            Color = p.Color;
            Dimension = p.Dimension;
            SkuSize = p.SkuSize;
            ReceivedPiecesMine = p.ReceivedPiecesMine;
            ExpectedPieces = p.ExpectedPieces == (int?)null ? 0 : p.ExpectedPieces;
            ExpectedCartonCount = p.ExpectedCartonCount == (int?)null ? 0 : p.ExpectedCartonCount;
            ReceivedCartonsMine = p.ReceivedCartonsMine;
            UploadDate = p.UploadDate;
            ShipmentDate = p.ShipmentDate;
            SewingPlantCode = p.SewingPlantCode;
            SewingPlantName = p.SewingPlantName;
            ReceivedCtnByBuddies = p.ReceivedCtnByBuddies;
            ReceivedCtnOfBuddies = p.ReceivedCtnOfBuddies == (int?)null ? 0 : p.ReceivedCtnOfBuddies;
            ReceivedPiecesByBuddies = p.ReceivedPiecesByBuddies;
            ReceivedPiecesOfBuddies = p.ReceivedPiecesOfBuddies == (int?)null ? 0 : p.ReceivedPiecesOfBuddies;
            CountOtherShipments = p.CountOtherShipments ?? 0;
            MinBuddyShipmentId = p.MinBuddyShipmentId;
            MaxBuddyShipmentId = p.MaxBuddyShipmentId;
            CountBuddyShipments = p.CountBuddyShipments ?? 0;
            IntransitType = p.IntransitType;
            TotalShipmentCount = p.TotalShipmentCount;

        }

        public int? TotalShipmentCount { get; set; }

        [Display(ShortName = "Shipment", Order = 10)]
        public string ShipmentId { get; set; }

        [ScaffoldColumn(false)]
        public string MinOtherShipmentId { get; set; }

        [ScaffoldColumn(false)]
        public string MaxOtherShipmentId { get; set; }

        [ScaffoldColumn(false)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountOtherShipments { get; set; }

        [ScaffoldColumn(false)]
        public string MinBuddyShipmentId { get; set; }

        [ScaffoldColumn(false)]
        public string MaxBuddyShipmentId { get; set; }

        [ScaffoldColumn(false)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountBuddyShipments { get; set; }

        [Display(Name = "Shipment Date", Order = 13)]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? ShipmentDate { get; set; }

        [Display(Name = "Sewing Plant Code")]
        [ScaffoldColumn(false)]
        public string SewingPlantCode { get; set; }

        [Display(Name = "Sewing Plant Name")]
        [ScaffoldColumn(false)]
        public string SewingPlantName { get; set; }

        [Display(ShortName = "Sewing Plant", Order = 130)]
        public string SewingPlant
        {
            get
            {
                return string.Format("{0} {1}", this.SewingPlantCode, this.SewingPlantName);
            }
        }

        [Display(ShortName = "Sent to ERP", Order = 20)]
        [DisplayFormat(DataFormatString = "{0:d}", NullDisplayText = "None")]

        public DateTime? UploadDate { get; set; }

        [Display(ShortName = "Style", Order = 30)]
        public string Style { get; set; }

        [Display(ShortName = "Color", Order = 40)]
        public string Color { get; set; }

        [Display(ShortName = "Dimension", Order = 50)]
        public string Dimension { get; set; }

        [Display(ShortName = "SKU Size", Order = 60)]
        public string SkuSize { get; set; }

        [ScaffoldColumn(false)]
        public string VwhId { get; set; }


        [Display(ShortName = "Pcs Expected", Order = 70)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? ExpectedPieces { get; set; }





        [Display(ShortName = "Ctns Expected", Order = 100)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? ExpectedCartonCount { get; set; }

        [ScaffoldColumn(false)]
        public int? ReceivedPiecesMine { get; set; }

        [Display(ShortName = "Pcs Received", Order = 80)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? ReceivedPieces
        {
            get
            {
                return (this.ReceivedPiecesMine ?? 0) + (this.ReceivedPiecesOfBuddies ?? 0);

            }
        }

        [ScaffoldColumn(false)]
        public int? ReceivedCartonsMine { get; set; }

        /// <summary>
        /// This is total received carton. Includes the received cartons of other shipment.
        /// </summary>
        [Display(ShortName = "Ctns Received", Order = 110)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? ReceivedCartons
        {
            get
            {
                return (this.ReceivedCartonsMine ?? 0) + (this.ReceivedCtnOfBuddies ?? 0);

            }
        }

        [ScaffoldColumn(false)]
        public string IntransitType { get; set; }

        [ScaffoldColumn(false)]
        public int? ReceivedCtnByBuddies { get; set; }


        [Display(ShortName = "Ctns Overage", Order = 100)]
        [ScaffoldColumn(false)]
        public int? ReceivedCtnOfBuddies { get; set; }

        [ScaffoldColumn(false)]
        //[DisplayFormat(DataFormatString = "{0:N0}")]
        public int? ReceivedPiecesByBuddies { get; set; }

        [Display(ShortName = "Pcs Overage", Order = 140)]
        [ScaffoldColumn(false)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? ReceivedPiecesOfBuddies { get; set; }


        /// <summary>
        /// Cartons not received in my shipment.
        /// </summary>
        [Display(ShortName = "Ctns Not Received", Order = 90)]
        [ScaffoldColumn(false)]
        public int? CartonsNotReceived
        {
            get
            {
                return (this.ExpectedCartonCount ?? 0) - (this.ReceivedCartonsMine ?? 0);

            }
        }

        [Display(ShortName = "Pcs Not Received", Order = 130)]
        [ScaffoldColumn(false)]
        public int? PiecesNotReceived
        {
            get
            {
                return (this.ExpectedPieces ?? 0) - (this.ReceivedPiecesMine ?? 0);
            }
        }

        [Display(ShortName = "Carton Variance", Order = 120)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? TotalCartonVariance
        {
            get
            {
                return (this.ReceivedCartons ?? 0) - (this.ExpectedCartonCount ?? 0);
            }
        }

        [Display(ShortName = "Pieces Variance", Order = 90)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? TotalPiecesVariance
        {
            get
            {
                return (this.ReceivedPieces ?? 0) - (this.ExpectedPieces ?? 0);
            }
        }

        [Display(ShortName = "Variance Commentary", Order = 140)]
        [DataType(DataType.MultilineText)]
        public string VarianceCommentsExcel
        {
            get
            {
                string x = string.Empty;
                if (this.CountBuddyShipments > 0)
                {

                    switch (this.CountBuddyShipments)
                    {
                        case 1:
                            x = string.Format("Received pieces include {0:N0} pieces of Shipment {1}.", this.ReceivedPiecesOfBuddies, this.MaxBuddyShipmentId);
                            break;
                        case 2:
                            x = string.Format("Received pieces include {0:N0} pieces of Shipments {1} and {2}.",
                                this.ReceivedPiecesOfBuddies, this.MaxBuddyShipmentId, this.MinBuddyShipmentId);
                            break;
                        default:
                            x = string.Format("Received pieces include {0:N0} pieces of Shipments {1}, {2} and {3} others.",
                                this.ReceivedPiecesOfBuddies, this.MaxBuddyShipmentId, this.MinBuddyShipmentId, this.CountBuddyShipments - 2);
                            break;
                    }
                }

                if (this.CountOtherShipments > 0)
                {

                    switch (this.CountOtherShipments)
                    {
                        case 1:
                            x = x + string.Format("{0:N0} pieces were received after closing against Shipment {1}.", this.ReceivedPiecesByBuddies, this.MaxOtherShipmentId);
                            break;
                        case 2:
                            x = x + string.Format("{0:N0} pieces were received after closing against Shipments {1} and {2}.",
                                this.ReceivedPiecesByBuddies, this.MaxOtherShipmentId, this.MinOtherShipmentId);
                            break;

                        default:
                            x = x + string.Format("{0:N0} pieces were received after closing against Shipments {1}, {2} and {3} others.",
                                this.ReceivedPiecesByBuddies, this.MaxOtherShipmentId, this.MinOtherShipmentId, this.CountOtherShipments - 2);
                            break;
                    }
                }

                return x;

            }
        }



    }



    public class ShipmentSkuFilterModelUnbinder : IModelUnbinder<ShipmentSkuFilterModel>
    {

        public void UnbindModel(RouteValueDictionary routeValueDictionary, string routeName, ShipmentSkuFilterModel model)
        {
            if (model.SkuStatus != ShipmentSkuStatusType.All)
            {
                routeValueDictionary.Add(model.NameFor(m => m.SkuStatus), model.SkuStatus);
            }

            if (model.SkuSource.HasValue)
            {
                routeValueDictionary.Add(model.NameFor(m => m.SkuSource), model.SkuSource);
            }

            if (!string.IsNullOrWhiteSpace(model.SewingPlantCode))
            {
                routeValueDictionary.Add(model.NameFor(m => m.SewingPlantCode), model.SewingPlantCode);
            }

            //if (model..VariancesOnly)
            //{
            //    routeValueDictionary.Add(model.NameFor(m => m.VariancesOnly), model.VariancesOnly);
            //}

            if (model.MinClosedDate.HasValue)
            {
                routeValueDictionary.Add(model.NameFor(m => m.MinClosedDate), model.MinClosedDate);
            }
            if (model.MaxClosedDate.HasValue)
            {
                routeValueDictionary.Add(model.NameFor(m => m.MaxClosedDate), model.MaxClosedDate);
            }


        }
    }
}