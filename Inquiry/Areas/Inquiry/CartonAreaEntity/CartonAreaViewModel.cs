using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonAreaEntity
{
    public class CartonAreaViewModel
    {
        //[Display(Name="Inventory in Area",ShortName = "Area Inventory")]
        //[AdditionalMetadata(ExcelAttribute.BUTTON_NAME, ExcelAttribute.BUTTON_NAME)]        
        public IList<CartonAreaInventoryModel> AreaInventory { get; set; }

        [Display(Name = "Total Locations")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalLocations { get; set; }

        [Display(Name = "Assigned Locations")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int AssignedLocations { get; set; }

        [Display(Name = "Non empty Locations")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NonEmptyLocations { get; set; }

        public int PercentFull
        {
            get
            {
                if (this.TotalLocations == 0)
                {
                    return 0;
                }
                return (int)Math.Round(this.NonEmptyLocations * 100.0 / this.TotalLocations);
            }
        }
        
        public string AreaId { get; set; }

        [Display(Name = "Area")]
        [Required(ErrorMessage = "Area should have Area ID")]
        public string ShortName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Location Numbering")]
        public bool LocationNumberingFlag { get; set; }

        [Display(Name = "Pallet Required")]
        public bool PalletRequired { get; set; }

        [Obsolete]
        public bool ShipableInventory { get; set; }

        [Display(Name = "Overdraft Allowed")]
        public bool OverdraftAllowed { get; set; }

        [Display(Name = "Repack Area")]
        public bool RepackArea { get; set; }

        [Display(Name = "Building")]
        public string WhID { get; set; }

        //public string PiecesReplenishLink { get; set; }

        //private readonly IList<DcmsLinkModel> _dcmsLinks = new List<DcmsLinkModel>();
        //public IList<DcmsLinkModel> DcmsLinks
        //{
        //    get
        //    {
        //        return _dcmsLinks;
        //    }
        //}

        public string UrlManageArea { get; set; }

    }
}