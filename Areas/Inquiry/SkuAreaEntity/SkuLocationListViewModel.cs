using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuAreaEntity
{


    public class SkuLocationHeadlineModel
    {
        public string LocationId { get; set; }

        public string IaId { get; set; }
        /// <summary>
        /// This property is added to conatin short name for Ia area.
        /// </summary>
        public string AreaShortName { get; set; }

        public string BuildingId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? MaxPieces { get; set; }

        public int SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string DisplaySku
        {
            get
            {
                return string.Format("{0}, {1}, {2}, {3}", Style, Color, Dimension, SkuSize);
            }
        }

    } 

    public class SkuLocationListViewModel
    {
        public IList<SkuLocationHeadlineModel> SkuLocList { get; set; }
    }
}