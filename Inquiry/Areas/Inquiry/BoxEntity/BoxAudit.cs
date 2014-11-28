using System;

namespace DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity
{
    internal class BoxAudit
    {
        public string ModuleCode { get; set; }

        public string FromIaId { get; set; }

        public string ActionPerformed { get; set; }

        public DateTime? DateCreated { get; set; }

        public string CreatedBy { get; set; }

        public string RejectionCode { get; set; }

        public string ToIaId { get; set; }

        public string ToPallet { get; set; }

        public string FromPallet { get; set; }

        public string ToLocation { get; set; }

        public string FromLocation { get; set; }


    }
}