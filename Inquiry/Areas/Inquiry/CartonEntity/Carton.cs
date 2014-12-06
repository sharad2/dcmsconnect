using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonEntity
{
    internal class ActiveCarton:CartonBase
    {
       

        public string Building { get; set; }

        public string CartonAreaId { get; set; }

        public string DamageCode { get; set; }

        public DateTime? LastPulledDate { get; set; }

        public string LocationId { get; set; }

        public string PalletId { get; set; }

        public string PriceSeasonCode { get; set; }

        public int? ReqProcessId { get; set; }

        public string ReservedUccID { get; set; }

        public string SewingPlantName { get; set; }

        public string SewingPlantCode { get; set; }

        public string ShipmentId { get; set; }

        public DateTime? ShipmentDate { get; set; }

        public DateTime? SuspenseDate { get; set; }

        public string UnmatchComment { get; set; }

        public string UnmatchReason { get; set; }

        public bool IsCartonMarkedForWork { get; set; }

        public string AreaShortName { get; set; }

        public string AreaDescription { get; set; }

        public string BestRestockAreaShortName { get; set; }

        public string BestRestockAisleId { get; set; }

        public string BestRestockBuildingId { get; set; }

        public string BestRestockLocationId { get; set; }
    }

    /// <summary>
    /// Basic information which applies to all types of cartons
    /// </summary>
    internal abstract class CartonBase
    {
        [Key]
        public string CartonId { get; set; }

        public int? SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string SkuSize { get; set; }

        public string Dimension { get; set; }

        public string VwhId { get; set; }

        public string QualityCode { get; set; }

        public string QualityDescription;

        public int? Pieces { get; set; }
    }

    internal class IntransitCarton: CartonBase
    {

        public string PriceSeasonCode { get; set; }

        public string SewingPlantName { get; set; }

        public string SewingPlantCode { get; set; }

        public int? SourceOrderedID { get; set; }

        public string SourceOrderPrefix { get; set; }

        public int? SourceOrderLineNumber { get; set; }

        public string ShipmentId { get; set; }

        public DateTime? ShipmentDate { get; set; }

        public int? IntransitId { get; set; }
    }

    internal class OpenCarton : CartonBase
    {

        public string CartonStorageArea { get; set; }

        public string ShortName { get; set; }

        public string LocationId { get; set; }

        public DateTime? LastPulledDate { get; set; }

        public string PalletId { get; set; }

        public string PriceSeasonCode { get; set; }

        public string DamageCode { get; set; }

        public string SewingPlantCode { get; set; }

        public string SewingPlantName { get; set; }

        public string ReservedUccID { get; set; }

        public string ShipmentId { get; set; }

        public DateTime? ShipmentDate { get; set; }
    }


}



//$Id$