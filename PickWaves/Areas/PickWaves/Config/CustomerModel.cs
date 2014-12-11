using System;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Config
{
    public class CustomerModel : IComparable<CustomerModel>, IEquatable<CustomerModel>
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int CompareTo(CustomerModel other)
        {
            return other.CustomerId.CompareTo(this.CustomerId);
        }

        public bool Equals(CustomerModel other)
        {
            return CompareTo(other) == 0;
        }

        public override int GetHashCode()
        {
            return CustomerId.GetHashCode();
        }
    }
}