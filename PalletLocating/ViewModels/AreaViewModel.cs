using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EclipseLibrary.Mvc.ModelBinding;

namespace DcmsMobile.PalletLocating.ViewModels
{
    public class AreaViewModel : ViewModelBase
    {
        public AreaViewModel()
        {
            this.UseSuggestionCache = true;
        }

        [BindUpperCase]
        public string AreaId { get; set; }

        [Display(Name = "Area")]
        [BindUpperCase]
        public string AreaShortName { get; set; }


        [BindUpperCase]
        [DisplayFormat(NullDisplayText = "Multiple Bldg")]
        public string BuildingId { get; set; }

        public IEnumerable<AreaModel> AreaList { get; set; }

        /// <summary>
        /// Whether replenishment suggestions should be displayed from the cache. Defaults to true.
        /// </summary>
        public bool UseSuggestionCache { get; set; }

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