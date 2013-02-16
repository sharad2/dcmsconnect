using System.Collections.Generic;
using System.Web.Mvc;

namespace DcmsMobile.DcmsLite.ViewModels.Pick
{
    public class BatchViewModel : ViewModelBase
    {
        public string BatchId
        {
            get
            {
                return CurrentPrintBatch != null ? CurrentPrintBatch.BatchId : string.Empty;
            }
        }

        public int? BucketId { get; set; }

        private IList<BoxModel> _boxList;
        public IList<BoxModel> BoxList
        {
            get
            {
                return _boxList ?? (_boxList = new List<BoxModel>(0));
            }
            set
            {
                _boxList = value;
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

        public PrintBatchModel CurrentPrintBatch { get; set; }

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

        public bool IsFrozenBucket { get; set; }
    }
}