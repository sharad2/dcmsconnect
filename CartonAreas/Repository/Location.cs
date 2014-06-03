
namespace DcmsMobile.CartonAreas.Repository
{
    public class Location
    {
        public string LocationId { get; set; }

        public int PalletCount { get; set; }

        public int CartonCount { get; set; }

        public int? TotalPieces { get; set; }

        public int? MaxAssignedCarton { get; set; }

        public Sku AssignedSku { get; set; }
        
        public string AssignedVwhId { get; set; }


        /// <summary>
        /// VWh Id of first carton at location 
        /// </summary>
        public string  CartonVwhId { get; set; }

        /// <summary>
        /// Count of all distinct VWh of cartons at location
        /// </summary>
        public int CartonVwhCount { get; set; }

        /// <summary>
        /// SKU of first carton at location 
        /// </summary>
        public Sku CartonSku { get; set; }

        /// <summary>
        /// Count of all distinct SKU of cartons at location
        /// </summary>
        public int CartonSkuCount { get; set; }


        public int CountTotalLocations { get; set; }
    }
}
//$Id$