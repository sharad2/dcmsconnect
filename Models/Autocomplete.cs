
namespace DcmsMobile.Repack.Models
{
    public class Autocomplete
    {
        /// <summary>
        /// Text displayed in the list
        /// </summary>
        public string label { get; set; }

        /// <summary>
        /// The id which is posted back (e.g. SKU Id)
        /// </summary>
        public string value { get; set; }

        /// <summary>
        /// Friendly short name of the selected value (such as UPC code). Defaults to value
        /// </summary>
        public string shortName { get; set; }
    }
}