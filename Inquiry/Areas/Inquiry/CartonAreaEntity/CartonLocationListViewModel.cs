using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonAreaEntity
{

    public class CartonLocationHeadlineModel
    {

        public string LocationId { get; set; }

        public string WhId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? Capacity { get; set; }

        public string Area { get; set; }

        public string ShortName { get; set; }
    }

    public class CartonLocationListViewModel
    {

        public IList<CartonLocationHeadlineModel> CartonLocationList { get; set; }
    }
}