using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PieceReplenish.Repository.Restock
{
    /// <summary>
    /// Validation attributes have been applied to this entity so that we can validate whether this carton is restockable
    /// </summary>
    internal class RestockCarton : IValidatableObject
    {
        private readonly DateTime _queryTime;

        public RestockCarton()
        {
            _queryTime = DateTime.Now;
        }

        public string CartonId { get; set; }

        /// <summary>
        ///  This property is required for capturing productivity for from_inventory_area
        /// </summary>
        public string CartonStorageArea { get; set; }

        /// <summary>
        /// Number of SKUs in carton
        /// </summary>
        [Range(1, 99999999, ErrorMessage = "Carton is empty")]
        public int PiecesInCarton { get; set; }

        /// <summary>
        /// The quality of carton
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "The quality of the carton is not known")]
        public string QualityCode { get; set; }

        /// <summary>
        /// Only cartons of these qualities can be restocked. This property is managed by the service.
        /// </summary>
        public IList<string> RestockableQualities { get; set; }

        /// <summary>
        /// The Virtual warehouse of carton
        /// </summary>
        public string VwhId { get; set; }

        /// <summary>
        /// Number of distinct SKUs in carton
        /// </summary>
        [Range(0, 1, ErrorMessage = "Carton contains multiple SKUs")]
        public int? SkuCount { get; set; }

        /// <summary>
        /// 0 means work is not needed. 1 means work is needed
        /// </summary>
        //[Range(0, 0, ErrorMessage = "Carton requires work. Cannot restock it.")]
        public bool IsWorkNeeded { get; set; }

        /// <summary>
        /// This property is required for capturing productivity for to_locationID
        /// </summary>
        public string RestockAtLocation { get; set; }

        /// <summary>
        /// This property is required for capturing productivity for warehouse_locationId
        /// </summary>
        public string BuildingId { get; set; }

        /// <summary>
        /// This property is required for capturing productivity for to_inventory_area
        /// </summary>
        public string PickAreaId { get; set; }

        public int PiecesPerPackage { get; set; }

        /// <summary>
        /// this property is needed for capturing productivity for number of units of SKU in carton
        /// </summary>
        public int? NumberOfUnits
        {
            get
            {
                if (PiecesPerPackage > 0)
                {
                    return PiecesInCarton / PiecesPerPackage;
                }

                return null;
            }
        }

        /// <summary>
        /// This property is managed by the service
        /// </summary>
        public IList<AssignedLocation> AssignedLocations { get; set; }

        /// <summary>
        /// 1 means open. 0 means not open
        /// </summary>
        [Range(0, 0, ErrorMessage = "Carton has already been restocked")]
        public int CartonType { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string LabelId { get; set; }

        /// <summary>
        /// Needed for productivity
        /// </summary>
        public string UpcCode { get; set; }

        public int? SkuId { get; set; }

        public decimal? RetailPrice { get; set; }

        /// <summary>
        /// Sharad 2 Nov 2013: Need to capture pallet id in productivity
        /// </summary>
        public string PalletId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // If restockable qualities have not been defined, we do not generate an error
            if (this.RestockableQualities != null && this.RestockableQualities.Count > 0 && !this.RestockableQualities.Contains(this.QualityCode))
            {
                yield return new ValidationResult(string.Format("Only qualities {0} can be restocked. Carton quality is {1}",
                    string.Join(",", RestockableQualities), this.QualityCode));
            }

            if (this.AssignedLocations.Count == 0)
            {
                var msg = string.Format("SKU {1},{2},{3},{4} VWh {5} of Carton {0} not assigned to any location",
                    this.CartonId, this.Style, this.Color, this.Dimension,
                    this.SkuSize, this.VwhId);
                yield return new ValidationResult(msg);

            }
        }
    }
}