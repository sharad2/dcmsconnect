
using System;
using System.ComponentModel.DataAnnotations;
namespace DcmsMobile.DcmsLite.Repository.Pick
{
    public class Bucket
    {
        [Key]
        public int BucketId { get; set; }

        [Key]
        public string PoId { get; set; }

        [Key]
        public string CustomerId { get; set; }

        [Key]
        public string VwhId { get; set; }

        [Key]
        public string CustomerDcId { get; set; }


        public string BucketName { get; set; }

        public int TotalBoxes { get; set; }

        public int TotalPickslips { get; set; }

        public int CountPrintedBoxes { get; set; }

        public DateTimeOffset CreationDate { get; set; }

        public string LastPrintedBy { get; set; }

        public DateTimeOffset? MinPrintedOn { get; set; }

        public DateTimeOffset? MaxPrintedOn { get; set; }

        public string CustomerName { get; set; }

        public bool IsFrozen { get; set; }

        public int? CountPrintedBy { get; set; }

        public int BoxesNotInBatch { get; set; }
    }
}