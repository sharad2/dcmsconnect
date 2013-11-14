
namespace DcmsMobile.PickWaves.Repository.Config
{
    public class CustomerSkuCase
    {
        /// <summary>
        /// Id of Customer who have overwritten splh
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Customer Name who have overwritten splh
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Id of Customer who have overwritten splh
        /// </summary>
        public string CaseId { get; set; }

        /// <summary>
        /// This property stores short description for case
        /// </summary>
        public string CaseDescription { get; set; }

        /// <summary>
        /// Max voume of box content.
        /// </summary>
        public decimal? MaxContentVolume { get; set; }

        /// <summary>
        /// Empty weight of box.
        /// </summary>
        public decimal? EmptyWeight { get; set; }

        /// <summary>
        /// This is comment that is gave while adding new customer sku case preference
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Puter volume of box content.
        /// </summary>
        public decimal? OuterCubeVolume { get; set; }
    }
}