using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuAreaEntity
{

    public class SkuLocationSkuModel
    {
        public SkuLocationSkuModel()
        {

        }

        internal SkuLocationSkuModel(SkuLocationSku entity)
        {
            Style = entity.Style;
            Color = entity.Color;
            Dimension = entity.Dimension;
            SkuSize = entity.SkuSize;
            Pieces = entity.Pieces;
            SkuId = entity.SkuId;
        }
        public int SkuId { get; set; }

        [Display(Name = "Style")]
        public string Style { get; set; }

        [Display(Name = "Color")]
        public string Color { get; set; }

        [Display(Name = "Dim")]
        public string Dimension { get; set; }

        [Display(Name = "Size")]
        public string SkuSize { get; set; }

        [Display(Name = "Pieces")]
        [DisplayFormat(DataFormatString="{0:N0}")]
        public int Pieces { get; set; }

        public string DisplaySku
        {
            get
            {
                return string.Format("{0}, {1}, {2}, {3}", Style, Color, Dimension, SkuSize);
            }
        }
    }

    public class SkuLocationPalletModel
    {
        public SkuLocationPalletModel()
        {

        }

        internal SkuLocationPalletModel(SkuLocationPallet entity)
        {
            this.PalletId = entity.PalletId;
            this.TotalBoxes = entity.TotalBoxes;
            this.CustomerName = entity.CustomerName;
        }

        [Display(Name = "Pallet")]
        [Required(ErrorMessage = "Pallet should have Pallet ID")]
        [RegularExpression(@"P\S{1,}", ErrorMessage = "Pallet ID must begin with a P")]
        [DataType("Alert")]
        public string PalletId { get; set; }

        [Display(ShortName = "Number of Boxes")]
        public int TotalBoxes { get; set; }

        [Required(ErrorMessage = "Customer should have name")]
        [Display(ShortName = "Customer")]
        [DataType("Alert")]
        public string CustomerName { get; set; }
    }

    public class SkuLocationViewModel
    {
        public SkuLocationViewModel()
        {

        }

        private bool _hasAssignedSku;
        internal SkuLocationViewModel(SkuLocation loc)
        {
            CycDate = loc.CycDate;
            CycStartDate = loc.CycStartDate;
            CycEndDate = loc.CycEndDate;
            IsCycMarked = !string.IsNullOrWhiteSpace(loc.CycFlag);
            IsFrozen = !string.IsNullOrWhiteSpace(loc.FreezeFlag);
            IaId = loc.IaId;
            ShortName = loc.AreaShortName;
            LocationId = loc.LocationId;
            MaxPieces = loc.MaxPieces ?? 0;
            PitchAisle = loc.PitchAisle;
            RestockAisle = loc.RestockAisle;
            VwhId = loc.VwhId;
            BuildingId = loc.BuildingId;
            AllSku = loc.SkusAtLocation.Select(p => new SkuLocationSkuModel(p)).ToList();
            _hasAssignedSku = loc.AssignedSkuId.HasValue;
            if (_hasAssignedSku)
            {
                AssignedStyle = loc.AssignedStyle;
                AssignedColor = loc.AssignedColor;
                AssignedDimension = loc.AssignedDimension;
                AssignedSkuSize = loc.AssignedSkuSize;
            }
        }
        public IList<SkuLocationSkuModel> AllSku { get; set; }

        public IList<SkuLocationPalletModel> AllPallets { get; set; }

        [Display(Name = "Location")]
        [Required(ErrorMessage = "Location should have Location ID")]
        [DataType("Alert")]
        public string LocationId { get; set; }

        [Display(Name = "Marked for CYC")]
        [DisplayFormat(NullDisplayText = "No")]
        public bool IsCycMarked { get; set; }

        [Display(Name = "CYC Date")]
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime? CycDate { get; set; }

        [Display(Name = "CYC Start Date")]
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime? CycStartDate { get; set; }

        [Display(Name = "CYC End Date")]
        [DisplayFormat(DataFormatString = "{0:t}")]
        public DateTime? CycEndDate { get; set; }

        public bool IsFrozen { get; set; }

        [Display(Name = "Capacity")]
        [DisplayFormat(NullDisplayText = "Not Defined", DataFormatString = "{0:N0}")]
        public int MaxPieces { get; set; }

        public int PercentFull
        {
            get
            {
                if (this.MaxPieces == 0)
                {
                    return 0;
                }
                return (int)Math.Round(this.TotalPieces * 100.0 / this.MaxPieces);
            }
        }



        [Display(Name = "Pitch Aisle")]
        [DisplayFormat(NullDisplayText = "Not Defined")]
        public string PitchAisle { get; set; }


        [Display(Name = "Restock Aisle")]
        [DisplayFormat(NullDisplayText = "Not Defined")]
        public string RestockAisle { get; set; }

        [Required(ErrorMessage = "Location does not belong to any inventory area")]
        [Display(Name = "Area", Description = "SKU area in which this location exists")]
        [DataType("Alert")]
        public string IaId { get; set; }

        public string ShortName { get; set; }

        [Display(Name = "VWh")]
        public string VwhId { get; set; }

        [Required(ErrorMessage = "Location does not belong to any Building")]
        [Display(Name = "Building")]
        [DataType("Alert")]
        public string BuildingId { get; set; }


        /// <summary>
        /// This property is used to capture the audit of assignment and  unassignment of SKU on location.
        /// </summary>        
        //public IList<LocationAuditModel> AssignUnassignAudit { get; set; }

        ///// <summary>
        ///// This property is used to capture the audit of pieces of SKU on location.
        ///// </summary>
        //[Obsolete]
        //public IList<LocationAuditModel> InventoryAudit { get; set; }

        [DisplayFormat(DataFormatString="{0:N0}")]
        public int TotalPieces
        {
            get
            {
                return this.AllSku.Sum(p => p.Pieces);
            }
        }

        public bool HasAssignedSku
        {
            get
            {
                return _hasAssignedSku;
            }
        }

        public string AssignedStyle { get; set; }

        public string AssignedColor { get; set; }

        public string AssignedDimension { get; set; }

        public string AssignedSkuSize { get; set; }

        /// <summary>
        /// Returns null if no SKU is assigned
        /// </summary>
        public string DisplayAssignedSku
        {
            get
            {
                return string.Format("{0}, {1}, {2}, {3}", AssignedStyle, AssignedColor, AssignedDimension, AssignedSkuSize);
            }
        }
    }
}