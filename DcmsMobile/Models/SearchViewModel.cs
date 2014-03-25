using System.Collections.Generic;
using System.Text;

namespace DcmsMobile.Models
{
    public class SearchResultModel
    {
        private readonly string _description;
        private readonly string _name;
        private readonly string _shortName;
        public SearchResultModel(AreaItem other, string searchText)
        {
            _description = HighlightSearchText(other.Description, searchText);
            _name = other.Name;
            _shortName = other.ShortName;
        }

        /// <summary>
        /// Method to highlight searchText.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="searchText"></param>
        /// <returns></returns>
        private string HighlightSearchText(string original, string searchText)
        {
            const string STRONG_START = "<strong>";
            const string STRONG_END = "</strong>";
            var sb = new StringBuilder();
            var startIndexName = 0;

            // Example Description: Restock Carton is used to restock carton’s content i.e. aSKUb in carton on a location where carton's cSKUd is assigned. 
            // Example Search Text SKU
            while (true)
            {
                var index = original.IndexOf(searchText, startIndexName, System.StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                {
                    break;
                }
                // First time in the loop, index is the index of a. Append everything before a.
                sb.Append(original, startIndexName, index - startIndexName);
                // Then append <strong>
                sb.Append(STRONG_START);
                // Then append number of characters matching search text
                sb.Append(original, index, searchText.Length);
                sb.Append(STRONG_END);
                startIndexName = index + searchText.Length;
            }
            sb.Append(original.Substring(startIndexName));
            return sb.ToString();
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }
        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string ShortName
        {
            get
            {
                return _shortName;
            }

        }

        /// <summary>
        /// Url which can be directly rendered
        /// </summary>
        public string Url { get; set; }
        public int Order { get; set; }
    }

    public class SearchViewModel:ViewModelBase
    {
        public string SearchText { get; set; }
        public IEnumerable<SearchResultModel> Results { get; set; }

        /// <summary>
        /// Current Limitation: Assume that Inquiry is the only program which can search for anything
        /// </summary>
        public string InquiryUrl { get; set; }
    }
}
