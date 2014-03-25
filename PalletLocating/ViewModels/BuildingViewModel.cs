using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EclipseLibrary.Mvc.ModelBinding;

namespace DcmsMobile.PalletLocating.ViewModels
{
    public class BuildingViewModel : ViewModelBase
    {

        /// <summary>
        /// Always returns uppercase
        /// </summary>
        [Display(Name = "Building")]
        [BindUpperCase]
        public string BuildingId { get; set; }

        public event EventHandler<EventArgs> AreaChoicesRequested;

        private IEnumerable<AreaModel> _areaChoices;

        /// <summary>
        /// We raise an event so that the controller can perform the query only if necessary
        /// </summary>
        public IEnumerable<AreaModel> AreaChoices
        {
            get
            {
                if (_areaChoices == null && AreaChoicesRequested != null)
                {
                    AreaChoicesRequested(this, EventArgs.Empty);
                }
                return _areaChoices;
            }
            set { _areaChoices = value; }
        }
        
        /// <summary>
        /// Give Summery info on Building Page.
        /// </summary>
        //public IEnumerable<InfoModel> SummaryInfo { get; set; }
       
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