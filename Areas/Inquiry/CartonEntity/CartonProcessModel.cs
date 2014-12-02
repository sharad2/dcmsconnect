using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonEntity
{
    public class CartonProcessModel
    {
        public CartonProcessModel()
        {

        }

        internal CartonProcessModel(CartonProcess entity)
        {
            this.ActionPerformed = entity.ActionPerformed;
            this.FromCartonArea = entity.FromCartonArea;
            this.FromLocation = entity.FromLocation;
            this.FromPalletId = entity.FromPalletId;
            this.InsertDate = entity.InsertDate;
            this.InsertedBy = entity.InsertedBy;
            this.ModuleCode = entity.ModuleCode;
            this.NewCartonQuantity = entity.NewCartonQuantity;
            this.NewSuspenseDate = entity.NewSuspenseDate;
            this.OldCartonQuantity = entity.OldCartonQuantity;
            this.OldSuspenseDate = entity.OldSuspenseDate;
            this.ToCartonArea = entity.ToCartonArea;
            this.ToLocation = entity.ToLocation;
            this.ToPalletId = entity.ToPalletId;            
        }

        [Required]
        [Display(ShortName = "Module",Order=2)]
        public string ModuleCode { get; set; }

        [Display(ShortName="Operator",Order=4)]
        public string InsertedBy { get; set; }

        [Display(ShortName = "From Pallet",Order=5)]
        public string FromPalletId { get; set; }

        [Display(ShortName = "To Pallet", Order = 6)]
        public string ToPalletId { get; set; }

        [Display(ShortName = "From Area", Order = 7)]
        [DisplayFormat(NullDisplayText = "None")]
        public string FromCartonArea { get; set; }

        [Display(ShortName = "To Area", Order = 8)]
        [DisplayFormat(NullDisplayText = "None")]
        public string ToCartonArea { get; set; }

        [Display(ShortName = "Date",Order=1)]
        [DataType(DataType.DateTime)]
        public DateTime? InsertDate { get; set; }

        [Display(ShortName = "Old Suspense Date", Order = 11)]
         public DateTime? OldSuspenseDate { get; set; }

        [Display(ShortName = "New Suspense Date", Order = 12)]
         public DateTime? NewSuspenseDate { get; set; }

        [Display(ShortName = "From Location", Order = 9)]
         public string FromLocation { get; set; }

        [Display(ShortName = "To Location", Order = 10)]
         public string ToLocation { get; set; }

        [Display(ShortName = "Action",Order=3)]
        public string ActionPerformed { get; set; }

        [Display(ShortName = "Old Quantity", Order = 13)]
        public int? OldCartonQuantity { get; set; }

        [Display(ShortName = "New Quantity", Order = 14)]
        public int? NewCartonQuantity { get; set; }
    }

    public interface ICartonProcessViewModel
    {
        IList<CartonProcessModel> ProcessList { get; }
    }
}