using System;
using System.Collections.Generic;
using System.Linq;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    internal class SparseMatrix
    {
        private IDictionary<Tuple<object, object>, Tuple<int, int>> _dict;

        public SparseMatrix()
        {
            _dict = new Dictionary<Tuple<object, object>, Tuple<int, int>>(20);
        }

        public void Add(object rowVal, object colVal, int pickslipCount, int orderedPieces)
        {
            _dict.Add(Tuple.Create(rowVal, colVal), Tuple.Create(pickslipCount, orderedPieces));
        }

        public IEnumerable<object> RowValues()
        {
            return _dict.Keys.Select(p => p.Item1).Distinct();
        }

        public IDictionary<string, int> PickslipCounts(object rowVal)
        {
            var query = (from item in _dict.Keys
                         where rowVal.Equals(item.Item1)
                         select new
                         {
                             Key = FormatValue(item.Item2),
                             Value = _dict[item].Item1
                         });
            var y = query.ToDictionary(p => p.Key, p => p.Value);
            return y;
        }

        public IDictionary<string, int> OrderedPieces(object rowVal)
        {
            var query = (from item in _dict.Keys
                         where rowVal.Equals(item.Item1)
                         select new
                         {
                             Key = FormatValue(item.Item2),
                             Value = _dict[item].Item2
                         });
            var y = query.ToDictionary(p => p.Key, p => p.Value);
            return y;
        }

        public static string FormatValue(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            if (value is DateTime)
            {
                return string.Format("{0:d}", value);
            }
            return value.ToString();
        }
    }

    /// <summary>
    /// Reprsents summarized information for imported order grouped by two specified columns
    /// </summary>
    internal class CustomerOrderSummary
    {
        /// <summary>
        /// Specifies the count of distinct values for each possible dimension. This will be null when there are no status 1 orders of the customer
        /// </summary>
        public IDictionary<PickslipDimension, int> CountValuesPerDimension { get; set; }



        public SparseMatrix AllValues2 { get; set; }

    }

}