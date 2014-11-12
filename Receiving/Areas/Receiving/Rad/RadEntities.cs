
using System;

namespace DcmsMobile.Receiving.Models.Rad
{

    public class SpotCheckConfiguration
    {
        public string Style { get; set; }

        public string SewingPlantId { get; set; }

        public int? SpotCheckPercent { get; set; }

        public string PlantName { get; set; }

        public string Color { get; set; }

        public bool? IsSpotCheckEnable { get; set; }

        public DateTimeOffset? CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset? ModifiedDate { get; set; }

        public string ModifiedBy { get; set; }
    }

    public class SewingPlant
    {
        public string SewingPlantCode { get; set; }

        public string PlantName { get; set; }

        //public string GroupingColumn { get; set; }

        //public string CountryName { get; set; }
    }

    //[Obsolete]
    //public class Style
    //{
    //    public string StyleId { get; set; }

    //    public string Description { get; set; }
    //}

    //[Obsolete]
    //public class Color
    //{
    //    public string ColorId { get; set; }

    //    public string Description { get; set; }
    //}

    public class SpotCheckArea
    {

        public string AreaId { get; set; }

        public string BuildingId { get; set; }
    }
}



//$Id$