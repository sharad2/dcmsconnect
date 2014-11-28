using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonAreaEntity
{
    public class CartonAreaInventoryModel
    {

        public CartonAreaInventoryModel()
        {
        
        }

        internal CartonAreaInventoryModel(CartonAreaInventory entity)
        {
            this.CartonCount = entity.CartonCount;
            //this.CartonId = entity.CartonId;
            this.DistinctSKUs = entity.DistinctSKUs;
            this.LabelId = entity.LabelId;
            //this.PalletId = entity.PalletId;
            this.SKUQuantity = entity.SKUQuantity;
        }

        [Display(Name = "Label")]
        [DataType("Alert")]
        [DisplayFormat(NullDisplayText = "--")]
        public string LabelId { get; set; }

        [Display(Name = "# Carton")]
        [DataType("Alert")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CartonCount { get; set; }

        [Display(Name = "# SKU")]
        [DataType("Alert")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? DistinctSKUs { get; set; }

        [Display(Name = "Quantity")]
        [DataType("Alert")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? SKUQuantity { get; set; }

        //[Display(Name = "Pallet")]
        //[ScaffoldColumn(false)]        
        //public string PalletId { get; set; }

        //[Display(Name = "Carton")]        
        //[ScaffoldColumn(false)]
        //public string CartonId { get; set; }
    }
}