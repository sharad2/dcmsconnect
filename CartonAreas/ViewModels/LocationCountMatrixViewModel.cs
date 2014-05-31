using DcmsMobile.CartonAreas.Repository;
using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    /// <summary>
    /// Used to display the counts of empty/assigned locations in a specific area
    /// </summary>
    public class LocationCountMatrixViewModel
    {
        /// <summary>
        /// This is a 3 by 3 2D array. Traditional 2D arrays require int indexes. Elements in this specialized array can be accessed using a bool? index.
        /// </summary>
        private class BoolArrayTwoDimensional
        {
            private readonly int?[,] _data;
            public BoolArrayTwoDimensional()
            {
                _data = new int?[3, 3];
            }

            /// <summary>
            /// Null maps to index 2. True maps to index 0. False Maps to Index 1.
            /// </summary>
            /// <param name="b"></param>
            /// <returns></returns>
            private static int ToArrayIndex(bool? b)
            {
                if (!b.HasValue)
                {
                    return 2;
                }
                return b.Value ? 0 : 1;
            }

            public int? this[bool? row, bool? col]
            {
                get { return _data[ToArrayIndex(row), ToArrayIndex(col)]; }
                set
                {
                    _data[ToArrayIndex(row), ToArrayIndex(col)] = value;
                }
            }

            [Obsolete]
            public void SetValue(bool? row, bool? col, int? value)
            {
                _data[ToArrayIndex(row), ToArrayIndex(col)] = value;
            }

            //public int? GetValue(bool? row, bool? col)
            //{
            //    return _data[ToArrayIndex(row), ToArrayIndex(col)];
            //}

        }

        /// <summary>
        /// First dimension is Assigned/Unassigned/Total
        /// Second Dimension is Empty/Non Empty/Total
        /// </summary>
        private readonly BoolArrayTwoDimensional _counts;

        public LocationCountMatrixViewModel()
        {
            _counts = new BoolArrayTwoDimensional();
        }

        internal LocationCountMatrixViewModel(CartonArea area)
        {
            _counts = new BoolArrayTwoDimensional();
            AreaId = area.AreaId;
            //TotalLocations = area.TotalLocations;
            _counts.SetValue(null, null, area.TotalLocations);
            CountAssignedLocations = area.CountAssignedLocations;
            _counts.SetValue(true, null, area.CountAssignedLocations);
            CountEmptyAssignedLocations = area.CountEmptyAssignedLocations;
            _counts.SetValue(true, true, area.CountEmptyAssignedLocations);
            CountEmptyUnassignedLocations = area.CountEmptyUnassignedLocations;
            _counts.SetValue(false, true, area.CountEmptyUnassignedLocations);
            CountEmptyLocations = area.CountEmptyLocations;
            _counts.SetValue(null, true, area.CountEmptyLocations);
            CountNonemptyAssignedLocations = area.CountNonemptyAssignedLocations;
            _counts.SetValue(true, false, area.CountNonemptyAssignedLocations);
            CountNonemptyUnassignedLocations = area.CountNonemptyUnassignedLocations;
            _counts.SetValue(false, false, area.CountNonemptyUnassignedLocations);
            CountUnassignedLocations = area.CountUnassignedLocations;
            _counts.SetValue(false, null, area.CountUnassignedLocations);
            CountNonemptyLocations = area.CountNonemptyLocations;
            _counts.SetValue(null, false, area.CountNonemptyLocations);
        }

        /// <summary>
        /// Filter applied for Assigned locations
        /// </summary>
        public bool? AssignedLocationsFilter { get; set; }

        /// <summary>
        /// Filter applied for Empty locations
        /// </summary>
        public bool? EmptyLocationsFilter { get; set; }

        public int? GetCount(bool? assigned, bool? empty)
        {
            return _counts[assigned, empty];
        }

        public string GetDisplayCount(bool? assigned, bool? empty)
        {
            return string.Format("{0:N0}", _counts[assigned, empty]);
        }

        [Obsolete]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountEmptyAssignedLocations { get; set; }

        [Obsolete]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountNonemptyAssignedLocations { get; set; }

        [Obsolete]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountAssignedLocations { get; set; }

        [Obsolete]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountEmptyUnassignedLocations { get; set; }

        [Obsolete]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountNonemptyUnassignedLocations { get; set; }

        [Obsolete]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountUnassignedLocations { get; set; }

        [Obsolete]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountEmptyLocations { get; set; }

        [Obsolete]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountNonemptyLocations { get; set; }

        //[Obsolete]
        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //public int? TotalLocations { get; set; }

        public string AreaId { get; set; }
    }
}