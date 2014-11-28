using System;

namespace DcmsMobile.Inquiry.Helpers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
    public class SearchQueryAttribute:Attribute
    {
        private readonly string _query;

        private static int __idGenerator;

        private readonly int _routingId;

        public SearchQueryAttribute(string query)
        {
            _query = query;
            _routingId = ++__idGenerator;
        }

        public string Query
        {
            get
            {
                return _query;
            }
        }

        /// <summary>
        /// Each attribute is guaranteed to have a unique value of RoutingId
        /// </summary>
        public int RoutingId
        {
            get
            {
                return _routingId;
            }
        }

        /// <summary>
        /// This comes into play when multiple matches are found. Within each group, only the highest rated match is retained.
        /// </summary>
        public int Rating { get; set; }

        public string Group { get; set; }

    }
}