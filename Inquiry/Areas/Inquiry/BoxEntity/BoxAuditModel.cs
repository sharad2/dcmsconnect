using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity
{
    public class BoxAuditModel
    {

        public BoxAuditModel()
        {

        }

        internal BoxAuditModel(BoxAudit entity)
        {
            this.ActionPerformed = entity.ActionPerformed;
            this.DateCreated = entity.DateCreated;
            this.CreatedBy = entity.CreatedBy;
            this.ModuleCode = entity.ModuleCode;
            this.RejectionCode = entity.RejectionCode;
            this.ToIaId = entity.ToIaId;
            this.FromIaId = entity.FromIaId;
            this.ToLocation = entity.ToLocation;
            this.FromLocation = entity.FromLocation;
            this.ToPallet = entity.ToPallet;
            this.FromPallet = entity.FromPallet;            
        }


        [Display(Name = "Action")]
        public string ActionPerformed { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.DateTime)]
        public DateTime? DateCreated { get; set; }


        [Display(Name = "Operator")]
        public string CreatedBy { get; set; }

        [Display(Name = "Module")]
        public string ModuleCode { get; set; }

        [Display(Name = "QC Rejection Code")]
        public string RejectionCode { get; set; }

        [Display(Name = "From Area")]
        public string FromIaId { get; set; }

        [Display(Name = "To Area")]
        public string ToIaId { get; set; }

        [Display(Name = "From Location")]
        public string FromLocation { get; set; }

        [Display(Name = "To Location")]
        public string ToLocation { get; set; }

        [Display(Name = "From Pallet")]
        public string FromPallet { get; set; }
        
        [Display(Name = "To Pallet")]
        public string ToPallet { get; set; }
        
    }
}