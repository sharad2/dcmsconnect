using System;
using System.Collections.Generic;

namespace DcmsMobile.Shipping.Repository
{
    /// <summary>
    /// The key against which routing information is stored
    /// </summary>
    internal class RoutingKey
    {

        private readonly string _customerId;
        private readonly string _poId;
        private readonly int _iteration;
        private readonly string _dcId;
        private readonly string _key;
        public RoutingKey(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                var tokens = key.Split(',');
                _poId = tokens[0];
                _iteration = int.Parse(tokens[1]);
                _customerId = tokens[2];
                _dcId = tokens[3];
            }
            _key = key;
        }

        public static RoutingKey Create(string customerId, string poId, int iteration, string dcId)
        {
            var key = (poId ?? string.Empty) + "," + iteration + "," + customerId + "," + dcId;
            return new RoutingKey(key);
        }

        public string Key
        {
            get
            {
                return _key;
            }
        }


        public string PoId
        {
            get
            {
                return _poId;
            }
        }

        public int Iteration
        {
            get
            {
                return _iteration;
            }
        }

        public string CustomerId
        {
            get
            {
                return _customerId;
            }
        }
        public string DcId
        {
            get
            {
                return _dcId;
            }
        }
    }
    /// <summary>
    /// Class used to update routing information
    /// </summary>
    internal class RoutingUpdater
    {

        public ICollection<RoutingKey> RoutingKeys { get; set; }

        public bool UpdateCarrierId { get; set; }

        public string CarrierId { get; set; }

        public bool UpdateCustomerDcId { get; set; }

        public string CustomerDcId { get; set; }

        public bool UpdateLoadId { get; set; }

        public string LoadId { get; set; }

        public bool UpdatePickupDate { get; set; }

        public DateTime? PickUpDate { get; set; }

        /// <summary>
        /// Whether anything needs to be updated
        /// </summary>
        public bool UpdateRequired
        {
            get
            {
                return UpdateCarrierId || UpdateCustomerDcId || UpdateLoadId || UpdatePickupDate;
            }
        }
    }
}
