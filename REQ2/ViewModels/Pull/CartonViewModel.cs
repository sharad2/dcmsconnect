using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.REQ2.ViewModels.Pull
{
    public class CartonViewModel
    {

        /// <summary>
        /// Pallet to keep cartons on
        /// </summary>
        private string _palletId;

        [Required]
        public string PalletId
        {
            get
            {
                return this._palletId;
            }
            set
            {
                this._palletId = value.ToUpper();
            }

        }
        /// <summary>
        /// TODO: Used another list CartonModel, create own list
        /// </summary>
        public IList<CartonModel> CartonList { get; set; }

        //[Required]
        //public string PalletId { get; set; }

        public string CartonId { get; set; }

        [Required]
        public string RequestId { get; set; }

        public string SourceShortName { get; set; }

        public string DestShortName { get; set; }
    }
}