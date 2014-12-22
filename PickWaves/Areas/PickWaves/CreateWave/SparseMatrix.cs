using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    /// <summary>
    /// Models a sparse 2D array. It can be thought of as a dictionary with two keys. Each cell of this dictionary can contain elements of type TElement
    /// </summary>
    internal class SparseMatrix<TKey1, TKey2, TElement>
    {
        private IDictionary<Tuple<TKey1, TKey2>, TElement> _dict;

        public SparseMatrix()
        {
            _dict = new Dictionary<Tuple<TKey1, TKey2>, TElement>(20);
        }

        /// <summary>
        /// The typle consists of rowVal, colVal, pickslip count and ordered pieces count respectively
        /// </summary>
        /// <param name="list"></param>
        public void Add(IEnumerable<Tuple<TKey1, TKey2, TElement>> list)
        {
            foreach (var item in list)
            {
                _dict.Add(Tuple.Create(item.Item1, item.Item2), item.Item3);
            }
        }

        /// <summary>
        /// Unique values of the first key
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TKey1> FirstKeys
        {
            get
            {
                return _dict.Keys.Select(p => p.Item1).Distinct();
            }
        }

        /// <summary>
        /// Returns a list of all values associated with the passed first key
        /// </summary>
        /// <param name="firstKeyVal"></param>
        /// <returns></returns>
        public IEnumerable<Tuple<TKey2, TElement>> FirstKeyValues(TKey1 firstKeyVal)
        {
            var query = from item in _dict
                        where firstKeyVal.Equals(item.Key.Item1)
                        select Tuple.Create(item.Key.Item2, item.Value);
            return query;
        }
    }

}