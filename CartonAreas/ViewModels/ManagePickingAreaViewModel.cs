using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    public class PickingAreaSkuModel
    {
        public int? SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string UpcCode { get; set; }

        public string DisplaySku
        {
            get
            {
                return string.Format("{0},{1},{2},{3}", Style, Color, Dimension, SkuSize);
            }
        }
    }

    public class PickingLocationModel
    {
        public string LocationId { get; set; }

        public int? TotalPieces { get; set; }

        public PickingAreaSkuModel AssignedSku { get; set; }

        public string AssignedVwhId { get; set; }        
    }

    public class ManagePickingAreaViewModel
    {
        [Required]
        public string AreaId { get; set; }

        public string ShortName { get; set; }

        public int CountTotalLocations { get; set; }

        public IList<PickingLocationModel> PickingLocations { get; set; }
    }
}