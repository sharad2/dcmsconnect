using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    /// <summary>
    /// Represents available information for each row of the dimension matrix
    /// </summary>
    public class RowDimensionModel
    {
        /// <summary>
        /// This is a work around for T4MVC. It is unable to generate reasonable action links when the value being passed in NULL.
        /// </summary>
        public const string NULL_DIMENSION_VALUE = "$NULL";

        private string _dimensionValue;

        /// <summary>
        /// This is the dimension value in string format which can be posted. E.g. this could be "12345"
        /// </summary>
        [Key]
        public string DimensionValue
        {
            get { return string.IsNullOrEmpty(_dimensionValue) ? NULL_DIMENSION_VALUE : _dimensionValue; }
            set { _dimensionValue = value; }
        }

        /// <summary>
        /// This is the dimension value which should be displayed. E.g. this could be "12,345"
        /// </summary>
        public string DimensionDisplayValue
        {
            get { return string.IsNullOrEmpty(_dimensionValue) ? "(Not specified)" : _dimensionValue; }
        }

        /// <summary>
        /// Key is Column Dimension. Value is pickslip count.
        /// </summary>        
        public IDictionary<string, int> PickslipCounts { get; set; }

        /// <summary>
        /// Total pickslips for this pickslip dimension
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalPickslips
        {
            get
            {
                return this.PickslipCounts.Values.Sum();
            }
        }

        public IDictionary<string, int> OrderedPieces { get; set; }
    }

}