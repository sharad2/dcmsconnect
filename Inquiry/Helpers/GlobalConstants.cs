
namespace DcmsMobile.Inquiry.Helpers
{
    /// <summary>
    /// Constants used by multiple controllers
    /// </summary>
    internal static class GlobalConstants
    {
        public const string COOKIE_UCC_CCL_PRINTER = "Inquiry_Ucc_Ccl_PrinterId";

        public const string COOKIE_PACKING_PRINTER = "Inquiry_Packingslip_PrinterId";

        public const string COOKIE_BOL_PRINTER = "Inquiry_Bol_PrinterId";

        /// <summary>
        /// Role required for managerial functions
        /// </summary>
        public const string ROLE_MANAGER = "DCMS8_POMGR";

        /// <summary>
        /// Max rows supported by excel sheet
        /// http://office.microsoft.com/en-in/excel-help/excel-specifications-and-limits-HP010342495.aspx
        /// </summary>
        public const int MAX_EXCEL_ROWS = 1048576;
    }
}