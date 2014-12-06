using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.DcmsLite.ViewModels.Receive
{
    public class IndexViewModel : ViewModelBase
    {
        public string IntransitId { get; set; }
       
        public string ShipmentId { get; set; }

        private IList<AsnModel> _asnList;
        public IList<AsnModel> AsnList
        {
            get
            {
                return _asnList ?? (_asnList = new AsnModel[0]);
            }
            set
            {
                _asnList = value;
            }
        }

        [Required(ErrorMessage = "Receiving area is required.")]
        [DisplayFormat(NullDisplayText="Unknown")]
        [Display(Name = "Receiving Area")]
        public string RecevingAreaId { get; set; }
    }
}