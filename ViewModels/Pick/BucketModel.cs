using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.DcmsLite.ViewModels.Pick
{
    public class BucketModel
    {
        [Key]
        [Display(Name = "Bucket")]
        public int BucketId { get; set; }

        public string BucketName { get; set; }

        [Display(Name = "#Boxes")]
        public int TotalBoxes { get; set; }

        [Display(Name = "#Pickslips")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalPickslips { get; set; }

        [DisplayFormat(DataFormatString="{0:N0}")]
        public int CountOfUccPrintedBoxes { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CountOfUccUnprintedBoxes
        {
            get
            {
                return this.TotalBoxes - this.CountOfUccPrintedBoxes;
            }
        }

        public int PercentPrinted
        {
            get
            {
                if (this.TotalBoxes == 0)
                {
                    return 0;
                }
                var pet = (int)((double)CountOfUccPrintedBoxes * 100 / this.TotalBoxes);
                return Math.Min(pet, 100);
            }
        }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTimeOffset CreationDate { get; set; }
        
        public string LastPrintedBy { get; set; }

        public int? CountPrintedBy { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d}")]
        public DateTimeOffset? MinPrintedOn { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTimeOffset? MaxPrintedOn { get; set; }

        public string Customer { get; set; }

        public string CustomerId { get; set; }

        public string VwhId { get; set; }

        public string PoId { get; set; }

        public string CustomerDcId { get; set; }

        public bool IsFrozen { get; set; }
    }
}