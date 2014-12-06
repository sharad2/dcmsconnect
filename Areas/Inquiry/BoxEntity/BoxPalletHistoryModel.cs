using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity
{
    public class BoxPalletHistoryModel
    {
        public BoxPalletHistoryModel()
        {

        }

        internal BoxPalletHistoryModel(BoxPalletHistory entity)
        {
            this.ModuleCode = entity.ModuleCode;
            this.Operation = entity.Operation;
            this.OperationStartDate = entity.OperationStartDate;
            this.Operator = entity.Operator;
            this.OutCome = entity.OutCome;
        }
        [Display(ShortName = "Operation Started On", Order = 2)]
        [DataType(DataType.DateTime)]
        public DateTime OperationStartDate { get; set; }

        [Display(ShortName = "Operator", Order = 4)]
        public string Operator { get; set; }

        [Display(ShortName = "OutCome", Order = 5)]
        public string OutCome { get; set; }

        [Display(ShortName = "Module", Order = 3)]
        public string ModuleCode { get; set; }

        [Display(ShortName = "Operation", Order = 1)]
        public string Operation { get; set; }
    }
}