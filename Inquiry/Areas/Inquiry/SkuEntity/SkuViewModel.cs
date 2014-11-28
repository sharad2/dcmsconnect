using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuEntity
{

    public class SkuPrivateLabelModel
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string ScannedBarCode { get; set; }

    }

    /// <summary>
    /// Inventory per Area/Vwh
    /// </summary>
    public class SkuInventoryModel
    {
        public SkuInventoryModel()
        {

        }

        internal SkuInventoryModel(SkuInventoryItem entity)
        {
            IaId = entity.IaId;
            Pieces = entity.Pieces ?? 0;
            LocationId = entity.LocationId;
            VwhId = entity.VwhId;
            ShortName = entity.ShortName;
            Building = entity.Building;
            CountLocations = entity.LocationCount;
            PiecesAtLocation = entity.PiecesAtLocation;
            Description = entity.AreaDescription;
        }

        [Display(Name = "Area")]
        [ScaffoldColumn(false)]
        public string IaId { get; set; }

        [ScaffoldColumn(false)]
        public string ShortName { get; set; }

        [Display(ShortName = "Area", Order = 2)]
        public string Description { get; set; }

        [Display(Name = "VWH", ShortName = "VWH", Order = 3)]
        public string VwhId { get; set; }

        [Display(Name = "Pieces", ShortName = "Pcs in Area", Order = 5)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Pieces { get; set; }

        [Display(Name = "#Locations", ShortName = "Loc's in Area", Order = 4)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CountLocations { get; set; }

        /// <summary>
        /// This property is added to hold pieces of location for an SKU
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "Pcs at top Loc", Order = 7)]
        public int PiecesAtLocation { get; set; }

        [Display(Name = "Location", ShortName = "Top Location", Order = 6)]
        public string LocationId { get; set; }

        /// <summary>
        /// This property is added to show building of area
        /// </summary>
        [DisplayFormat(NullDisplayText = "Not Defined")]
        [Display(Name = "Building", Order = 1)]
        public string Building { get; set; }

    }

    public class SkuViewModel
    {

        [Key]
        [ScaffoldColumn(false)]
        [Display(Name = "SKU ID")]
        public int SkuId { get; set; }

        [Required(ErrorMessage = "This SKU does not have a UPC")]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "Upc must be exactly 12 digits")]
        [Display(Name = "UPC")]
        [DataType("Alert")]
        [DisplayFormat(NullDisplayText = "None")]
        public string Upc { get; set; }

        [Required(ErrorMessage = "This SKU does not have a style")]
        [DisplayFormat(NullDisplayText = "No Style")]
        [Display(Name = "Style")]
        [DataType("Alert")]
        public string Style { get; set; }

        [Required(ErrorMessage = "This SKU does not have a color")]
        [DisplayFormat(NullDisplayText = "No Color")]
        [Display(Name = "Color")]
        [DataType("Alert")]
        public string Color { get; set; }

        [Required(ErrorMessage = "This SKU does not have a dimension")]
        [DisplayFormat(NullDisplayText = "No Dimension")]
        [Display(Name = "Dim")]
        [DataType("Alert")]
        public string Dimension { get; set; }

        [Required(ErrorMessage = "This SKU does not have a size")]
        [DisplayFormat(NullDisplayText = "No Size")]
        [Display(Name = "Size")]
        [DataType("Alert")]
        public string SkuSize { get; set; }

        [Display(Name = "Pieces Per Package")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesPerPackage { get; set; }

        [DisplayFormat(DataFormatString = "${0:N2}")]
        [Display(Name = "Retail Price")]
        public decimal? RetailPrice { get; set; }

        [Display(Name = " Additional Retail Price")]
        [DisplayFormat(DataFormatString="({0})")]
        public string AdditionalRetailPrice { get; set; }

        /// <summary>
        /// This property is added to show the descrition of UPC
        /// </summary>
        [Display(Name = "Description")]
        [DisplayFormat(NullDisplayText = "SKU Description not provided")]
        public string Description { get; set; }

        [Display(Name = "Standard Case Qty")]
        [DisplayFormat(NullDisplayText = "N/A", DataFormatString = "{0:N0}")]
        public int? StandardCaseQty { get; set; }

        /// <summary>
        /// This Property will contain sum of pieces of SKU.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalPieces
        {
            get
            {
                return this.SkuAreaInventory.Sum(p => p.Pieces);
            }

        }

        [Display(ShortName = "Sku inventory in Area/Vwh", Order = 1)]
        public IList<SkuInventoryModel> SkuAreaInventory { get; set; }

        [Display(ShortName = "List of Customers that have private label for SKU", Order = 2)]
        public IList<SkuPrivateLabelModel> CustomerLabelList { get; set; }

    }
}




//$Id$