
namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuAreaEntity
{
    internal class SkuAreaHeadline
    {
        public string IaId { get; set; }

        public string ShortName { get; set; }

        public string Description { get; set; }

        public int? NumberOfLocations { get; set; }
    }

    internal class SkuArea : SkuAreaHeadline
    {

        public string DefaultLocation { get; set; }

        public string WhId { get; set; }

        public string PickingAreaFlag { get; set; }

        public string ShipingAreaFlag { get; set; }

        public int? PullCartonLimit { get; set; }

        public int? AssignedLocations { get; set; }    
    
    }


}



//$Id$