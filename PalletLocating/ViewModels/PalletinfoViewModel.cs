using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PalletLocating.ViewModels
{
    public class PalletinfoViewModel : ViewModelBase
    {
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        public string Pallet { get; set; }

        public int CountCarton { get; set; }

        public string FromArea { get; set; }

        public string ToArea { get; set; }

        public string FromLocation { get; set; }

        public string ToLocation { get; set; }
                
        public DateTime InsertDate { get; set; }

        [Display(Name = "To")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? InsertToDate { get; set; }

        [Display(Name = "From")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? InsertFromDate { get; set; }

        public IEnumerable<PalletMovementModel> PalletInfo { get; set; }
     }
}


    //$Id$ 
    //$Revision$
    //$URL$
    //$Header$
    //$Author$
    //$Date$
