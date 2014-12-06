using DcmsMobile.Inquiry.Helpers;
using System;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuEntity
{
    /// <summary>
    /// All information that we know about the SKU
    /// </summary>
    /// <remarks>
    /// Validation rules exist for each property through attributes. An SKU is valid when ...
    /// </remarks>
    internal class Sku:SkuBase
    {
        public string Upc { get; set; }

        public int? PiecesPerPackage { get; set; }

        public decimal? RetailPrice { get; set; }

        public string AdditionalRetailPrice { get; set; }

        /// <summary>
        /// This property is added to show the descrition of UPC
        /// </summary>
        public string Description { get; set; }

        public int? StandardCaseQty{get;set;}
       
    }

    internal class SkuInventoryItem
    {
        /// <summary>
        /// True if area originated in tab_inventory_area, false if it originated in ia.
        /// </summary>
        public bool? IsCartonArea { get; set; }

        public string IaId { get; set; }

        public string ShortName { get; set; }

        public string Building { get; set; }

        public string LocationId { get; set; }

        /// <summary>
        /// This property is added to hold pieces of location for an SKU
        /// </summary>
        public int PiecesAtLocation { get; set; }

        public int LocationCount { get; set; }

        public int? Pieces { get; set; }

        public string VwhId { get; set; }

        public string AreaDescription { get; set; }
    }

    internal class SkuAutoComplete:SkuBase
    {
        public string Upc { get; set; }
    }

    internal class SkuHeadline : SkuBase
    {
        public string Upc { get; set; }

        public DateTime? PickslipOrderDate { get; set; }

    }
}




//$Id$