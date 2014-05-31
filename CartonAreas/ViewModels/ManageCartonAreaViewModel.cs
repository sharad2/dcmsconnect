using DcmsMobile.CartonAreas.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{

    public class SkuModel
    {
        [Display(Name = "SKU")]
        [Required]
        public int? SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        [Display(Name = "Upc Code")]
        public string UpcCode { get; set; }
    }

    public class LocationModel
    {
        public LocationModel()
        {

        }

        internal LocationModel(Location entity)
        {
            if (entity.AssignedSku != null)
            {
                AssignedSku = new SkuModel
                        {
                            Style = entity.AssignedSku.Style,
                            Color = entity.AssignedSku.Color,
                            Dimension = entity.AssignedSku.Dimension,
                            SkuSize = entity.AssignedSku.SkuSize,
                            SkuId = entity.AssignedSku.SkuId,
                            UpcCode = entity.AssignedSku.UpcCode
                        };
            }
            if (entity.CartonSku != null)
            {
                CartonSku = new SkuModel
                                                    {
                                                        Style = entity.CartonSku.Style,
                                                        Color = entity.CartonSku.Color,
                                                        Dimension = entity.CartonSku.Dimension,
                                                        SkuSize = entity.CartonSku.SkuSize,
                                                        SkuId = entity.CartonSku.SkuId,
                                                        UpcCode = entity.CartonSku.UpcCode
                                                    };
            }
            CartonCount = entity.CartonCount;
            PalletCount = entity.PalletCount;
            AssignedVwhId = entity.AssignedVwhId;
            TotalPieces = entity.TotalPieces;
            LocationId = entity.LocationId;
            MaxAssignedCartons = entity.MaxAssignedCarton;
            CartonSkuCount = entity.CartonSkuCount;
        }

        [Required]
        [Display(Name = "Location")]
        public string LocationId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "Total Pallets")]
        public int PalletCount { get; set; }

        [Display(Name = "VWh")]
        public string AssignedVwhId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "Assigned Cartons")]
        public int CartonCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "Total Pieces")]
        public int? TotalPieces { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "Max Cartons")]
        [Range(minimum: 1, maximum: 99999, ErrorMessage = "Cartons must be in between 1 to 99999")]
        public int? MaxAssignedCartons { get; set; }

        public SkuModel AssignedSku { get; set; }

        public SkuModel CartonSku { get; set; }

        public int PercentFull
        {
            get
            {
                if (this.MaxAssignedCartons == null || this.MaxAssignedCartons <= 0)
                {
                    return 0;       // Not full at all
                }
                var pct = (int)((double)this.CartonCount * 100.0 / (double)this.MaxAssignedCartons);
                return Math.Min(pct, 100);
            }
        }

        public int CartonSkuCount { get; set; }
    }

    public class ManageCartonAreaViewModel
    {

        public string BuildingId { get; set; }

        public string ShortName { get; set; }

        public IList<LocationModel> Locations { get; set; }

        public LocationCountMatrixViewModel Matrix { get; set; }

        /// <summary>
        /// The location pattern which was used to filter the list
        /// </summary>
        public string LocationPatternFilter { get; set; }

        /// <summary>
        /// This will be non null if the Assigned to SKU filter has been used
        /// </summary>
        public SkuModel AssignedToSkuFilter { get; set; }

        /// <summary>
        /// Used to post the Assign SKU dialog
        /// </summary>
        public AssignSkuViewModel AssignedSku { get; set; }

    }
}

//$Id$ 
//$Revision$
//$URL$
//$Header$
//$Author$
//$Date$
