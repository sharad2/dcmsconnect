using DcmsMobile.Inquiry.Helpers;
using System;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuAreaEntity
{
    internal class LocationAudit : SkuBase
    {

        public DateTime? DateCreated { get; set; }

        public string CreatedBy { get; set; }

        public string ActionPerformed { get; set; }

        public string ModuleCode { get; set; }

        public int? Pieces { get; set; }

        public int? TransactionPieces { get; set; }

        public string UpcCode { get; set; }

    }
}