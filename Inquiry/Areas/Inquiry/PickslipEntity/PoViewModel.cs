using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity
{
    public class PoViewModel : IPickslipListViewModel
    {
        public PoViewModel()
        {

        }

        internal PoViewModel(PurchaseOrder entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            this.TotalBoxes = entity.TotalBoxes;
            this.CountOfUccPrinted = entity.CountOfUccPrinted;
            this.CountOfCclPrinted = entity.CountOfCclPrinted;
            this.CustomerId = entity.CustomerId;
            this.CustomerName = entity.CustomerName;
            this.Iteration = entity.Iteration;
            this.CountIterations = entity.CountIterations;
            this.StartDate = entity.StartDate;
            this.CancelDate = entity.CancelDate;
            this.DcCancelDate = entity.DcCancelDate;
            this.PoId = entity.PoId;            

        }

        private IList<PickslipHeadlineModel> _allPickslips;
      
        public IList<PickslipHeadlineModel> AllPickslips
        {
            get { return _allPickslips ?? (_allPickslips = new List<PickslipHeadlineModel>(0)); }
            set
            {
                _allPickslips = value;
            }
        }

        /// <summary>
        /// A descriptive title for the model. Contains information on how the data was retrieved.
        /// Suitable for displaying as page title.
        /// </summary>
        public string ModelTitle { get; set; }

        internal int? PickslipLimit { get; set; }

        public string PickslipListCaption
        {
            get
            {
                if (PickslipLimit.HasValue)
                {
                    return string.Format("Only {0:N0} of many pickslips are listed", PickslipLimit);
                }
                else
                {
                    return string.Format("{0:N0} pickslips", this.AllPickslips.Count);
                }
            }
        }


        [Required(ErrorMessage = "PO ID is required.")]
        [Display(Name = "PO")]
        [DataType("Alert")]
        public string PoId { get; set; }

        [DisplayFormat(DataFormatString = "Iteration {0}")]
        public int Iteration { get; set; }

        [Display(Name = "Customer")]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerDisplayName
        {
            get
            {
                return string.Format("{0} : {1}", this.CustomerId, this.CustomerName);
            }
        }
     
        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Cancel Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? CancelDate { get; set; }

        [Display(Name = "DC Cancel Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? DcCancelDate { get; set; }
        
        public int CountOfCclPrinted { get; set; }

        public int CountOfUccPrinted { get; set; }

        public int TotalBoxes { get; set; }

        /// <summary>
        /// How many iterations exist for this PO
        /// </summary>
        public int CountIterations { get; set; }

        //public bool ShowInventoryStatus { get; set; }

    }
}