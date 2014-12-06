namespace DcmsMobile.PieceReplenish.ViewModels
{
    public class SkuModel
    {
        public int SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string UpcCode { get; set; }


        public override string ToString()
        {
            return string.Format(Style + "," + Color + "," + Dimension + "," + SkuSize);
        }

        public int AisleCapacity { get; set; }

        public int? PiecesInAisle { get; set; }

        public int? PiecesInRestock { get; set; }

        public int PiecesInPullableCarton { get; set; }

        public int? PiecesToPick { get; set; }

        public int? WavePriority { get; set; }

        public int? SkuReplenishmentPriority { get; set; }

        public string VwhId { get; set; }

        public int CartonsToPull { get; set; }

        public int? WaveCount { get; set; }

        public int PercentInAisle { get; set; }

        public int PercentInRestock { get; set; }

        public int PercentToPull { get; set; }

        public int? CartonsInRestock { get; set; }

        /// <summary>
        /// True if the SKU is currently assigned to a puller for pulling
        /// </summary>
        public bool BeingPulled { get; set; }

        /// <summary>
        /// True if this SKU is a candidate for getting assigned to the next puller
        /// </summary>
        public bool WillGetPulledNext { get; set; }

        public bool IsOverpulling
        {
            get
            {
                return AisleCapacity < (PiecesInAisle ?? 0) + (PiecesInRestock ?? 0) + PiecesInPullableCarton;
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

