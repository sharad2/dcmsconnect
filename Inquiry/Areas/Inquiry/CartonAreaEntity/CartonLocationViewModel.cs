using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonAreaEntity
{
    public class CartonAtLocationModel
    {
       
        [DisplayFormat(DataFormatString = "Pallet {0}",NullDisplayText="No Pallet")]
        public string PalletId { get; set; }

        [Display(Name = "Quantity", Order = 30)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? SKUQuantity { get; set; }

        [Display(Name = "Carton", Order = 10)]
        public string CartonId { get; set; }
    }

    public class CartonLocationViewModel
    {
        [Display(Name = "Location")]
        public string LocationId { get; set; }

        [Display(Name = "Area")]
        public string ShortName { get; set; }

        public string Area { get; set; }

        [Display(Name = "Building")]
        public string WhId { get; set; }

        [Display(Name = "Capacity")]
        [DisplayFormat(NullDisplayText = "Not Set", DataFormatString = "{0:N0}")]
        public int? Capacity { get; set; }

      
        //[StringLength(12, MinimumLength = 12, ErrorMessage = "Upc must be exactly 12 digits")]
        //[Display(Name = "UPC")]      
        //[DisplayFormat(NullDisplayText = "None")]
        //public string AssignedUpc { get; set; }

         [DisplayFormat(NullDisplayText = "Not Assigned")]
        [Display(Name="Assigned SKU")]
        public string DisplayAssignedSku 
        {
            get 
            {               
                    return string.Format("{0}, {1}, {2}, {3}", this.AssignedStyle, this.AssignedColor, this.AssignedDimension, this.AssignedSkuSize);
            
            }
        }
        
       // [DisplayFormat(NullDisplayText = "No Style")]
        [Display(Name = "Style")]        
        public string AssignedStyle { get; set; }

        
       // [DisplayFormat(NullDisplayText = "No Color")]
        [Display(Name = "Color")]        
        public string AssignedColor { get; set; }

        
       // [DisplayFormat(NullDisplayText = "No Dimension")]
        [Display(Name = "Dim")]
        public string AssignedDimension { get; set; }

        
      //  [DisplayFormat(NullDisplayText = "No Size")]
        [Display(Name = "Size")]        
        public string AssignedSkuSize { get; set; }

        public IList<CartonAtLocationModel> Cartons { get; set; }

        //public IList<CartonLocationPalletModel> PalletCartons { get; set; }
                
        public int TotalCarton { get; set; }     
    }
}