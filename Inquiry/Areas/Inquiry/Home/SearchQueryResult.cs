using DcmsMobile.Inquiry.Helpers;

namespace DcmsMobile.Inquiry.Areas.Inquiry.Home
{
    /// <summary>
    /// Contains information which allows us to route to a specific action if the associated query returns any result.
    /// This class is constructed by reading the SearchQueryAttibute defined on actions.
    /// </summary>
    internal class SearchRouting
    {

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public SearchQueryAttribute SearchAttr { get; set; }
    }

    /// <summary>
    /// Stores results of the search query
    /// </summary>
    internal class SearchQueryResult
    {
        public int RoutingId { get; set; }

        /// <summary>
        /// A readable description of ScanType. Displayed when the user is asked to disambiguate a scan.
        /// </summary>
        public string ScanDescription { get; set; }

        /// <summary>
        /// The id to be used to lookup the information for search text. This may or may not be the same as the search text.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// To resolve ambiguity, these additional keys need to be applied to the query
        /// </summary>
        public string PrimaryKey1 { get; set; }
        public string PrimaryKey2 { get; set; }
    }
}




//$Id$