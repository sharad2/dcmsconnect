
namespace DcmsMobile.DcmsLite.Repository.Home
{
    internal enum SearchTextType
    {
        Unrecognized,
        Ucc,
        PrintBatch,
        PickWave
    }
    internal class ScanInfo
    {
        /// <summary>
        /// The key which should be passed to the page to which we are redirecting
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// TODO: should be enum to deduce the scan type
        /// </summary>
        internal SearchTextType ScanType { get; set; }
    }
}