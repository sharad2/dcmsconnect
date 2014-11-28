using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuEntity
{
    public class LabelViewModel
    {
        [Display(Name = "LabelId")]
        public string LabelId { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

    }
}



//$Id$