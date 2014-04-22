using System;
using System.Collections.Generic;

namespace DcmsMobile.PickWaves.Repository.CreateWave
{
    internal class CustomerOrderSummary
    {
        /// <summary>
        /// The value in the column which was chosen as the dimension for the pickslip
        /// </summary>
        /// <remarks>
        /// The datatype of the value depends on the dimension which was retrieved
        /// </remarks>
        public object DimensionValue { get; set; }

        /// <summary>
        /// Type of the value in DimensionValue
        /// </summary>
        public Type DimensionValueType { get; set; }

        /// <summary>
        /// Key is DC
        /// </summary>
        public IDictionary<object, int> Data { get; set; }

        public IDictionary<PickslipDimension, int> Counts { get; set; }
    }
}