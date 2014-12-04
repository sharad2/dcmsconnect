
using System;

namespace DcmsMobile.PalletLocating.Models
{
    public class Area
    {
        public string AreaId { get; set; }

        public string Description { get; set; }

        public string ReplenishAreaId { get; set; }

        public string ShortName { get; set; }

        public string ReplenishAreaShortName { get; set; }

        public string BuildingId { get; set; }

        public bool IsNumbered { get; set; }

        ///// <summary>
        ///// These property is used for give 
        ///// summery of located pallets by user.
        ///// </summary>
        //public string UserName { get; set; }

        //public int PalletCount { get; set; }

        //public int CartonCount { get; set; }

        //public string FromArea { get; set; }

        //public string ToArea { get; set; }

        //public DateTime LocatingDate { get; set; }


        /// <summary>
        /// The time at which query was performed
        /// </summary>
        public DateTime QueryTime { get; set; }
    }

    /// <summary>
    /// Area statistics
    /// </summary>
    //public class AreaStatistics : Area
    //{
    //    public int PalletCount { get; set; }

    //    public int CartonCount { get; set; }

    //    public int LocationCount { get; set; }

    //    public int UsedLocationCount { get; set; }

    //    public int AssignedLocationCount { get; set; }

    //    public int CartonCapacity { get; set; }
    //}
}


/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/