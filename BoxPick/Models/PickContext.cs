
namespace DcmsMobile.BoxPick.Models
{
    public class PickContext
    {
        public string BuildingId { get; set; }

        public string SourceArea { get; set; }

        public string SourceAreaShortName { get; set; }

        public bool IsBuilding
        {
            get
            {
                return !string.IsNullOrEmpty(this.BuildingId) && string.IsNullOrEmpty(SourceArea);
            }
        }

        public bool IsSingleBuildingArea
        {
            get
            {
                return !string.IsNullOrEmpty(BuildingId) && !string.IsNullOrEmpty(SourceArea);
            }
        }

        public bool IsMultiBuildingArea
        {
            get
            {
                return string.IsNullOrEmpty(BuildingId) && !string.IsNullOrEmpty(SourceArea);
            }
        }
    }
}



//$Id$