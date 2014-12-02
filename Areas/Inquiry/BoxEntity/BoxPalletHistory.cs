using System;

namespace DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity
{
    internal class BoxPalletHistory
    {
        public DateTime OperationStartDate { get; set; }

        public string Operator { get; set; }

        public string OutCome { get; set; }

        public string ModuleCode { get; set; }

        public string Operation { get; set; }
    }
}