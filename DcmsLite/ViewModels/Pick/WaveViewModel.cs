using System.Collections.Generic;
using System.Web.Mvc;

namespace DcmsMobile.DcmsLite.ViewModels.Pick
{
    public class WaveViewModel : ViewModelBase
    {
        public int BucketId { get; set; }

        public bool IsFrozen { get; set; }

        private IList<SelectListItem> _printerList;

        public IList<SelectListItem> PrinterList
        {
            get
            {
                return _printerList ?? (_printerList = new List<SelectListItem>(0));
            }
            set
            {
                _printerList = value;
            }
        }

        public int TotalBoxes { get; set; }

        public int PrintedBoxCount { get; set; }

        public int UnprintedBoxCount
        {
            get
            {
                return TotalBoxes - PrintedBoxCount;
            }
        }

        private IList<PrintBatchModel> _printedBatchList;
        public IList<PrintBatchModel> PrintedBatchList
        {
            get
            {
                return _printedBatchList ?? (_printedBatchList = new List<PrintBatchModel>(0));
            }

            set
            {
                _printedBatchList = value;
            }
        }

        public int BoxesNotInBatch { get; set; }

        /// <summary>
        /// Batch size remembered in cookie
        /// </summary>
        public int? BatchSize { get; set; }
    }
}