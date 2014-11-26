using System;
using System.Collections.Generic;
using System.Linq;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    internal class CellValue
    {
        public int PickslipCount { get; set; }

        public int OrderedPieces { get; set; }
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

        /// <summary>
        /// This is a 2D array of cell values.
        /// </summary>
        public Matrix AllValues { get; set; }

    }


    /// <summary>
    /// 2D array
    /// </summary>
    /// <remarks>
    /// Implemented as dictionary of dictionaries
    /// </remarks>
    internal class Matrix
    {
        private readonly IDictionary<object, IDictionary<object, CellValue>> _dict;

        public Matrix()
        {
            _dict = new Dictionary<object, IDictionary<object, CellValue>>(12);
        }
        /// <summary>
        /// Adds all column values for a specific row
        /// </summary>
        /// <param name="rowVal"></param>
        /// <param name="colVals"></param>
        public void AddRow(object rowVal, IDictionary<object, CellValue> colVals)
        {
            _dict.Add(rowVal, colVals);
        }

        /// <summary>
        /// Returns all column values for the passed row
        /// </summary>
        /// <param name="rowVal"></param>
        /// <returns></returns>
        public IDictionary<object, CellValue> GetRow(object rowVal)
        {
            return _dict[rowVal];
        }

        /// <summary>
        /// Enumerate distinct row values
        /// </summary>
        public ICollection<object> RowValues
        {
            get
            {
                return _dict.Keys;
            }
        }

        /// <summary>
        /// Enumerate distinct col values
        /// </summary>
        public IEnumerable<object> ColValues
        {
            get
            {
                return _dict.Values.SelectMany(p => p.Keys).Distinct();
            }
        }
    }
}