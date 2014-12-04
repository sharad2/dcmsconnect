using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.BoxManager.Repository
{
    public class Customer
    {
        [Key]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }
    }
}