using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Routing;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.Inquiry.Areas.Inquiry.IntransitEntity
{
    /// <summary>
    /// Where is this shipment coming from ?
    /// </summary>
    public enum ShipmentSourceType
    {
        Vendor,
        Transfer
    }

    public enum ShipmentStatusType
    {
        Open,
        Closed,
        Dates
    }

    public class ShipmentListFilterModel
    {
        public ShipmentListFilterModel()
        {

        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other"></param>
        public ShipmentListFilterModel(ShipmentListFilterModel other)
        {
            Source = other.Source;
            Status = other.Status;
            SewingPlantCode = other.SewingPlantCode;
            VariancesOnly = other.VariancesOnly;
            MinClosedDate = other.MinClosedDate;
            MaxClosedDate = other.MaxClosedDate;
        }

        [DisplayFormat(NullDisplayText="All Shipment Types")]
        public ShipmentSourceType? Source { get; set; }

        public ShipmentStatusType Status { get; set; }

        public string SewingPlantCode { get; set; }

        public bool VariancesOnly { get; set; }

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
                switch (Status)
                {
                    case ShipmentStatusType.Open:
                        return "Open";
                    case ShipmentStatusType.Closed:
                        return "Closed";
                    case ShipmentStatusType.Dates:
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
                return Source != null || Status != ShipmentStatusType.Open || !string.IsNullOrWhiteSpace(SewingPlantCode) ||
                    VariancesOnly || MinClosedDate != null || MaxClosedDate != null;
            }
        }

        public string DisplayFilters
        {
            get
            {
                //return "TODO: One line summary of applied filters";
                List<string> list = new List<string>();
                if (this.Status != ShipmentStatusType.Open)
                {
                    list.Add(this.Status.ToString());
                }
                if (this.Source.HasValue)
                {
                    list.Add(this.Source.ToString());
                }
                if (this.MinClosedDate != null || this.MaxClosedDate != null)
                {
                    list.Add("Dates");
                }
                if (!string.IsNullOrWhiteSpace(this.SewingPlantCode))
                {
                    list.Add(string.Format("Sewing Plant {0}", SewingPlantCode));
                }
                if (this.VariancesOnly)
                {
                    list.Add("Variances");
                }
                return string.Join(", ", list);
            }
        }

    }

    public class IntransitShipmentListViewModel
    {
        private ShipmentListFilterModel _filters;
        /// <summary>
        /// These filters will be posted
        /// </summary>
        public ShipmentListFilterModel Filters
        {
            get
            {
                if (_filters == null)
                {
                    _filters = new ShipmentListFilterModel();
                }
                return _filters;
            }
            set
            {
                _filters = value;
            }
        }

        public IList<IntransitShipmentModel> Shipments { get; set; }

        public IList<SelectListItem> SewingPlantList { get; set; }

    }

    public class ShipmentListFilterModelUnbinder : IModelUnbinder<ShipmentListFilterModel>
    {

        public void UnbindModel(RouteValueDictionary routeValueDictionary, string routeName, ShipmentListFilterModel model)
        {
            if (model.Status != ShipmentStatusType.Open)
            {
                routeValueDictionary.Add(model.NameFor(m => m.Status), model.Status);
            }

            if (model.Source.HasValue)
            {
                routeValueDictionary.Add(model.NameFor(m => m.Source), model.Source);
            }

            if (!string.IsNullOrWhiteSpace(model.SewingPlantCode))
            {
                routeValueDictionary.Add(model.NameFor(m => m.SewingPlantCode), model.SewingPlantCode);
            }

            if (model.VariancesOnly)
            {
                routeValueDictionary.Add(model.NameFor(m => m.VariancesOnly), model.VariancesOnly);
            }

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