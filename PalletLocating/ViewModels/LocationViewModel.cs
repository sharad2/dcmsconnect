using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PalletLocating.ViewModels
{
    public class LocationViewModel : ViewModelBase
    {
        public IList<CartonLocationModel> SuggestedLocations { get; set; }

        [Display(Name = "Location")]
        [Required(ErrorMessage = "Location is required")]
        public string LocationId { get; set; }

        [Required(ErrorMessage = "This message should be never shown that PalletId is always required")]
        public string PalletId { get; set; }

        [Display(Name = "Cartons on Pallet")]
        public int PalletCartonCount { get; set; }

        public int PalletSkuCount { get; set; }

        /// <summary>
        /// Display representation of the SKU on pallet
        /// </summary>
        [Display(Name ="SKU on Pallet")]
        public SkuModel PalletSku { get; set; }

        /// <summary>
        /// The area suggested by the system. Locations of this area will be displayed
        /// </summary>
        //public string SuggestedAreaId { get; set; }

        //[Required(ErrorMessage = "This message should be never shown that Building is always required")]
        [DisplayFormat(NullDisplayText = "Multiple Bldg")]
        public string BuildingId { get; set; }

        [Required(ErrorMessage = "This message should be never shown that TargetAreaId is always required")]
        public string TargetAreaId { get; set; }

        /// <summary>
        /// The area where the user wants to locate
        /// </summary>
        public string TargetAreaShortName { get; set; }

        /// <summary>
        /// The replenishment area associated with the Target Area
        /// </summary>
        public string ReplenishAreaShortName { get; set; }

        public string SuggestedAreaId { get; set; }

        public object SuggestedAreaShortName { get; set; }

        /// <summary>
        /// The location entered by the user to confirm to locate the pallet.
        /// This is checked if confirmation is needed.
        /// </summary>
        public string ConfirmLocationId { get; set; }
        
        /// <summary>
        /// VWh count of all cartons on the pallet
        /// </summary>
        public int CartonVwhCount { get; set; }

        /// <summary>
        /// VWh of Carton on the Pallet
        /// </summary>
        [Display(Name = "VWh of Carton")]
        public string CartonVwhId { get; set; }

        /// <summary>
        /// The area in which the pallet exists
        /// </summary>
        [Display(Name = "Pallet Area")]
        public string PalletAreaShortName { get; set; }

        [Display(Name = "Pallet Location")]
        public string PalletLocation { get; set; }

        public string PalletToMerge { get; set; }
    }
}


/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/