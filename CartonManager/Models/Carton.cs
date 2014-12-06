using System;

namespace DcmsMobile.CartonManager.Models
{
    /// <summary>
    /// What task needs to be performed for the carton
    /// </summary>
    [Flags]
    public enum CartonUpdateFlags
    {
        /// <summary>
        /// No task needs to be performed
        /// </summary>
        None = 0,

        /// <summary>
        /// SKU of the carton must be checked
        /// </summary>
        Sku = 0x1,

        /// <summary>
        /// Pieces of the carton need to be checked
        /// </summary>
        Pieces = 0x2,

        /// <summary>
        /// Virtual warehouse of the carton must be checked
        /// </summary>
        Vwh = 0x4,

        /// <summary>
        /// Quality of the carton must be checked
        /// </summary>
        Quality = 0x8,

        /// <summary>
        /// Pallet of the carton must be checked
        /// </summary>
        Pallet = 0x10,

        /// <summary>
        /// Area of the carton must be checked
        /// </summary>
        Area = 0x20,

        /// <summary>
        /// Remark of the carton should be checked
        /// </summary>
        MarkReworkComplete = 0x40,

        /// <summary>
        /// The carton should be un marked for rework.
        /// </summary>
        /// <remarks>
        /// This flag should not be set together with MarkReworkComplete. If both flags are set, the service will raise an error.
        /// </remarks>
        AbandonRework = 0x80,

        /// <summary>
        /// Location of carton
        /// </summary>
        Location = 0x100,

        /// <summary>
        /// Price season code of carton
        /// </summary>
        PriceSeasonCode = 0x200,

        /// <summary>
        /// Remove Carton fron Pallet
        /// </summary>
        RemovePallet = 0x400,

      
        UpdateTasks = CartonUpdateFlags.PriceSeasonCode | CartonUpdateFlags.Sku | CartonUpdateFlags.Quality | CartonUpdateFlags.Pieces | CartonUpdateFlags.Vwh | CartonUpdateFlags.MarkReworkComplete | CartonUpdateFlags.AbandonRework,

        MoveTasks = CartonUpdateFlags.Pallet | CartonUpdateFlags.Area | CartonUpdateFlags.Location | CartonUpdateFlags.RemovePallet

    }

    /// <summary>
    /// Properties of a carton
    /// </summary>
    public class Carton
    {
        /// <summary>
        /// The unique ID of the carton.
        /// </summary>
        public string CartonId { get; set; }

        /// <summary>
        /// SKU contained in the carton. Assumes that the carton can only contain one SKU
        /// </summary>
        public Sku SkuInCarton { get; set; }

        /// <summary>
        /// Number of pieces in the carton
        /// </summary>
        public int Pieces { get; set; }

        /// <summary>
        /// Virtual warehouse of the carton
        /// </summary>
        public string VwhId { get; set; }

        /// <summary>
        /// Quality code of the carton
        /// </summary>
        public string QualityCode { get; set; }

        /// <summary>
        /// Pallet on which the carton exists
        /// </summary>
        public string PalletId { get; set; }

        /// <summary>
        /// The inventory area in which the carton exists
        /// </summary>
        public CartonArea CartonArea { get; set; }

        /// <summary>
        /// Remark associated with the carton
        /// </summary>
        public bool RemarkWorkNeeded { get; set; }

        /// <summary>
        /// Location of carton
        /// </summary>
        public string LocationId { get; set; }

        public string ReasonCode { get; set; }

        public string PriceSeasonCode { get; set; }

        public bool IsReserved { get; set;}

        public string BundleId { get; set; }
    }



}



//$Id$