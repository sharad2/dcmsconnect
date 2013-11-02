using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.REQ2.ViewModels.Pull
{


    public class PalletViewModel
    {

        /// <summary>
        /// Pallet to keep cartons on
        /// </summary>
        private string _palletId;

        [ReadOnly(false)]
        [RegularExpression(@"^([P|p]\S{1,7})", ErrorMessage = "Pallet Id must begin with P and max length should be less then 9.")]
        public string PalletId
        {
            get
            {
                return this._palletId;
            }
            set
            {
                _palletId = value != null ? value.ToUpper() : null;
            }
        }

        [Required]
        [ReadOnly(false)]
        public string RequestId { get; set; }

        [ReadOnly(true)]
        public string SourceShortName { get; set; }

        [ReadOnly(true)]
        public string DestShortName { get; set; }

        [ReadOnly(true)]
        public string Requestedby { get; set; }

    }
}