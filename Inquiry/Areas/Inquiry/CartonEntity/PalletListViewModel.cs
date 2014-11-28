using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonEntity
{

    public class PalletHeadLineModel
    {
        public string PalletId { get; set; }

        public int? TotalCarton { get; set; }

        public DateTime? MaxAreaChangeDate { get; set; }

        public DateTime? MinAreaChangeDate { get; set; }

        public int CartonAreaCount { get; set; }

        public string AreaShortName { get; set; }

        public string WarehouseLocationId { get; set; }
        
        public string DisplayAreaChangeDate
        {
            get
            {
               //Date and time is different.
                if (this.MaxAreaChangeDate.ToString() != this.MinAreaChangeDate.ToString())
                {
                    //Date is same but time is different.
                    if (this.MaxAreaChangeDate.Value.Hour == this.MinAreaChangeDate.Value.Hour)
                    {
                        return string.Format("{0} to {1:T}", this.MaxAreaChangeDate, this.MinAreaChangeDate);
                    }
                    return string.Format("{0} to {1}", this.MaxAreaChangeDate, this.MinAreaChangeDate);

                }
                // Date and time is same.
                return string.Format("{0}",this.MaxAreaChangeDate.ToString());
            }
        }


    }
    public class CartonPalletListViewModel
    {
        public IList<PalletHeadLineModel> PalletList { get; set; }

    }
}