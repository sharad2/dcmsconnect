using DcmsMobile.CartonAreas.Repository;
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
        public PickingLocationModel()
        {

        }

        internal PickingLocationModel(PickingLocation entity)
        {
            if (entity.AssignedSku != null)
            {
                AssignedSku = new PickingAreaSkuModel
                        {
                            Style = entity.AssignedSku.Style,
                            Color = entity.AssignedSku.Color,
                            Dimension = entity.AssignedSku.Dimension,
                            SkuSize = entity.AssignedSku.SkuSize,
                            SkuId = entity.AssignedSku.SkuId,
                            UpcCode = entity.AssignedSku.UpcCode
                        };
            }
            AssignedVwhId = entity.AssignedVwhId;
            TotalPieces = entity.TotalPieces;
            LocationId = entity.LocationId;
            MaxAssignedPieces = entity.MaxAssignedPieces;
        }
        public string LocationId { get; set; }

        public int TotalPieces { get; set; }

        public PickingAreaSkuModel AssignedSku { get; set; }

        public string AssignedVwhId { get; set; }

        public int MaxAssignedPieces { get; set; }

        public int PercentFullLocation
        {
            get
            {
                if (TotalPieces == 0 || MaxAssignedPieces == 0)
                {
                    return 0;
                }
                return TotalPieces * 100 / MaxAssignedPieces;
            }
        }
    }

    public class ManagePickingAreaViewModel
    {
        [Required]
        public string AreaId { get; set; }

        public string ShortName { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CountTotalLocations { get; set; }

        public PickingAreaLocationCountMatrixViewModel Matrix { get; set; }

        public IList<PickingLocationModel> PickingLocations { get; set; }

        [Display(Name = "Building")]
        public string BuildingId { get; set; }

        /// <summary>
        /// The location pattern which was used to filter the list
        /// </summary>
        public string LocationPatternFilter { get; set; }

        /// <summary>
        /// This will be non null if the Assigned to SKU filter has been used
        /// </summary>
        public PickingAreaSkuModel AssignedToSkuFilter { get; set; }
    }
}