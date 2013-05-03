using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.Repository
{
    /// <summary>
    /// Encapsulates summary information associated with an ATS Date
    /// </summary>
    public class AtsDateSummary
    {
        [Key]
        public DateTime AtsDate { get; set; }

        public int? EdiId { get; set; }

        public int PoCount { get; set; }
    }
}