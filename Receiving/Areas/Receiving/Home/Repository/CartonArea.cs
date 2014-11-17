
namespace DcmsMobile.Receiving.Areas.Receiving.Home.Repository
{
    internal class CartonArea
    {
        //Building to display
        public string BuildingId { get; set; }
        public string AreaId { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public bool IsNumberedArea { get; set; }
        public bool IsReceivingArea { get; set; }
        public bool IsSpotCheckArea { get; set; }
    }
}