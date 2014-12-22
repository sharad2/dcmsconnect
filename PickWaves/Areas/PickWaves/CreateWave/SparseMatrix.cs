using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    /// <summary>
    /// Models a sparse 2D array. It can be thought of as a dictionary with two keys. Each cell of this dictionary can contain elements of type TElement
    /// </summary>
    public class SparseMatrix<TKey1, TKey2, TElement> : IEnumerable<KeyValuePair<Tuple<TKey1, TKey2>, TElement>>
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
        public void AddRange(IEnumerable<Tuple<TKey1, TKey2, TElement>> list)
        {
            foreach (var item in list)
            {
                _dict.Add(Tuple.Create(item.Item1, item.Item2), item.Item3);
            }
            // Get rid of the cached lists
            _list1 = null;
            _list2 = null;
        }

        private IList<TKey1> _list1;
        /// <summary>
        /// Unique values of the first key
        /// </summary>
        /// <returns></returns>
        public IList<TKey1> FirstKeys
        {
            get
            {
                if (_list1 == null)
                {
                    _list1 = _dict.Keys.Select(p => p.Item1).Distinct().ToList();
                }
                return _list1;
            }
        }

        private IDictionary<TKey1, IList<KeyValuePair<TKey2, TElement>>> _list2;
        /// <summary>
        /// Returns a list of all values associated with the passed first key
        /// </summary>
        /// <param name="firstKeyVal"></param>
        /// <returns></returns>
        public IList<KeyValuePair<TKey2, TElement>> this[TKey1 index]
        {
            get
            {
                if (_list2 == null)
                {
                    _list2 = new Dictionary<TKey1, IList<KeyValuePair<TKey2, TElement>>>();
                }
                IList<KeyValuePair<TKey2, TElement>> result;
                if (!_list2.TryGetValue(index, out result))
                {
                    result = (from item in _dict
                             where index.Equals(item.Key.Item1)
                             select new KeyValuePair<TKey2, TElement>(item.Key.Item2, item.Value)).ToList();
                    _list2.Add(index, result);
                }
                return result;
            }

        }

        public IEnumerator<KeyValuePair<Tuple<TKey1, TKey2>, TElement>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }
    }

}