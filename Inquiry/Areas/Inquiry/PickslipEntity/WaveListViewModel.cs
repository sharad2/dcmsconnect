using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity
{
    public class WaveHeadlineModel
    {
        public string PitchBuilding { get; set; }

        public string PitchArea { get; set; }

        public string PitchAreaDescription { get; set; }

        public string BuildingPullFrom { get; set; }

        public string PullArea { get; set; }

        public string PullAreaDescription { get; set; }

        public int BucketId { get; set; }

        public string BucketName { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime DateCreated { get; set; }

        public bool Freeze { get; set; }

        public string BucketStatus { get; set; }
       
        public int? Priority { get; set; }

        [Display(Name = "Customer")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}", NullDisplayText = "0")]
        public int? PickslipCount { get; set; }


        [DisplayFormat(DataFormatString = "{0:N0}", NullDisplayText = "0")]
        public int? PoCount { get; set; }
    }

    public class WaveListViewModel
    {
        public IList<WaveHeadlineModel> WaveList { get; set; }
    }
}
