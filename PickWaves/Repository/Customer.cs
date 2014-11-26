using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Repository
{
    internal class Customer : IEquatable<Customer>
    {
        [Key]
        public string CustomerId { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// False if the customer has been marked as inactive
        /// </summary>
        public bool IsActive { get; set; }

        public bool Equals(Customer other)
        {
            return this.CustomerId.Equals(other.CustomerId);
        }

        public override int GetHashCode()
        {
            return this.CustomerId.GetHashCode();
        }
    }
}