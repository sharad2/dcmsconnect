using DcmsMobile.PickWaves.Helpers;
using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Repository
{
    public class InventoryArea
    {
        [Key]
        public string AreaId { get; set; }

        /// <summary>
        /// Pitching area or pulling area?
        /// </summary>
        public BucketActivityType AreaType { get; set; }

        /// <summary>
        /// Short name of area.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Description of area.
        /// </summary>
        public string Description { get; set; }

        public string BuildingId { get; set; }

        public string BuildingName { get; set; }

        /// <summary>
        /// Carton area from which this pick area is replenished
        /// </summary>
        public string ReplenishAreaId { get; set; }
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
