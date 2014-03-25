using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using DcmsMobile.PalletLocating.Models;
using EclipseLibrary.Mvc.ModelBinding;

namespace DcmsMobile.PalletLocating.ViewModels
{
    public class PalletViewModel : ViewModelBase
    {
        public PalletViewModel() { }

        public PalletViewModel(Area area)
        {
            this.AreaId = area.AreaId;
            this.AreaShortName = area.ShortName;
            this.ReplenishmentAreaId = area.ReplenishAreaId;
            this.ReplenishAreaShortName = area.ReplenishAreaShortName;
            this.BuildingId = area.BuildingId;
            this.IsAreaNumbered = area.IsNumbered;
        }

        [Required(ErrorMessage = "Pallet is required")]
        [Display(Name = "Pallet/Carton")]
        [BindUpperCase]
        public string PalletOrCartonId { get; set; }

        [BindUpperCase]
        [DisplayFormat(NullDisplayText = "Multiple Bldg")]
        public string BuildingId { get; set; }

        /// <summary>
        /// The destination area entered by the user
        /// </summary>
        [Required(ErrorMessage = "This message should be never shown that AreaId is always required")]
        [HiddenInput(DisplayValue = false)]
        public string AreaId { get; set; }

        public bool IsAreaNumbered { get; set; }

        [HiddenInput(DisplayValue = true)]
        public string AreaShortName { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string ReplenishmentAreaId { get; set; }

        [HiddenInput(DisplayValue = true)]
        public string ReplenishAreaShortName { get; set; }

        /// <summary>
        /// List of suggested Locations for pallet to be pulled
        /// </summary>
        public IEnumerable<ReplenishmentSuggestionModel> SuggestedLocations { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string LastScan { get; set; }

        /// <summary>
        /// The time at which the suggestions were queried
        /// </summary>
        [DisplayFormat(DataFormatString="{0:t}")]
        public DateTime? SuggestionQueryTime
        {
            get
            {
                var first = this.SuggestedLocations.FirstOrDefault();
                return first == null ? (DateTime?) null : first.QueryTime;
            }
        }
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