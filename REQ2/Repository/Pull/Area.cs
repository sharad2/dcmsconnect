
namespace DcmsMobile.REQ2.Repository.Pull
{
    public class Area
    {

        public string DestinationAreaId { get; set; }

        public string SourceAreaId { get; set; }

        public string SourceAreaShortName { get; set; }

        public string DestinationAreaShortName { get; set; }

        public string SourceBuildingId { get; set; }

        public string DestinationBuildingId { get; set; }

        public int    PullableCartonCount { get; set; }

        public string TopRequestId { get; set; }


    }
}