using System.ComponentModel.DataAnnotations;
namespace DcmsMobile.CartonAreas.ViewModels
{
    public class AreaModel
    {
        public string AreaId { get; set; }

        public string Description { get; set; }

        public string ShortName { get; set; }

        [Display(Name = "Building")]
        [DisplayFormat(NullDisplayText = "(All)")]
        public string BuildingId { get; set; }
    }
}