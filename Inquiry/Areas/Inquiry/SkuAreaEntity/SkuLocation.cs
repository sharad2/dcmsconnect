using DcmsMobile.Inquiry.Helpers;
using System;
using System.Collections.Generic;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuAreaEntity
{
    /// <summary>
    /// SKUs at location
    /// </summary>
    internal class SkuLocationSku : SkuBase
    {
        public int Pieces { get; set; }
    }

    internal class SkuLocationHeadline : SkuBase
    {
        public string LocationId { get; set; }      

        public string IaId { get; set; }
        /// <summary>
        /// This property is added to conatin short name for Ia area.
        /// </summary>
        public string AreaShortName { get; set; }

        public string BuildingId { get; set; }

        public int? MaxPieces { get; set; }

    }


    internal class SkuLocation : SkuLocationHeadline
    { 
        
        public int? AssignedSkuId { get; set; }

        public IList<SkuLocationSku> SkusAtLocation { get; set; }

        public string CycFlag { get; set; }

        public DateTime? CycDate { get; set; }

        public string FreezeFlag { get; set; } 

        public string PitchAisle { get; set; }

        public string RestockAisle { get; set; }

        //this property is added to show assigned sku of that location
        //public Sku AssignedSku { get; set; }


        //public string AssignedUpc { get; set; }

        public string AssignedStyle { get; set; }

        public string AssignedColor { get; set; }

        public string AssignedDimension { get; set; }

        public string AssignedSkuSize { get; set; }

        public string VwhId { get; set; }

        public DateTime? CycStartDate { get; set; }

        public DateTime? CycEndDate { get; set; }
    }
}



//$Id$