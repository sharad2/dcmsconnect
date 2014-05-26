using System;

namespace DcmsMobile.CartonAreas.Repository
{
    /// <summary>
    /// For update address of any building
    /// </summary>
    internal class Address
    {
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Address4 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string Address3 { get; set; }

        public string CountryCode { get; set; }

    }

    internal class Building
    {
        public string BuildingId { get; set; }

        public string Description { get; set; }

        public DateTime? InsertDate { get; set; }

        public string InsertedBy { get; set; }       

        public Address Address { get; set; }

        public int? CountAreas { get; set; }

        public int? CountNumberedAreas { get; set; }

        public int? CountLocations { get; set; }

        public int? ReceivingPalletLimit { get; set; }        
    }
}