using System;

namespace DcmsMobile.DcmsLite.Repository.Pick
{
    public class Box
    {
        /// <summary>
        /// UCC id of this box
        /// </summary>
        public string UccId { get; set; }
        
        /// <summary>
        /// When the label of this box was printed last time
        /// </summary>
        public DateTime? LastUccPrintDate { get; set; }

        public string BoxId { get; set; }
    }
}