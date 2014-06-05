using DcmsMobile.CartonAreas.Repository;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    public class PickingAreaLocationCountMatrixViewModel
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

        }

        /// <summary>
        /// First dimension is Assigned/Unassigned/Total
        /// Second Dimension is Empty/Non Empty/Total
        /// </summary>
        private readonly BoolArrayTwoDimensional _counts;

        public PickingAreaLocationCountMatrixViewModel()
        {
            _counts = new BoolArrayTwoDimensional();
        }

        internal PickingAreaLocationCountMatrixViewModel(PickingArea area)
        {
            _counts = new BoolArrayTwoDimensional();
            AreaId = area.AreaId;
            _counts[null, null] = area.LocationCount;
            _counts[true, null] = area.CountAssignedLocations;
            _counts[true, true] = area.CountEmptyAssignedLocations;
            _counts[false, true] = area.CountEmptyUnassignedLocations;
            _counts[null, true] = area.CountEmptyLocations;
            _counts[true, false] = area.CountNonemptyAssignedLocations;
            _counts[false, false] = area.CountNonemptyUnassignedLocations;
            _counts[false, null] = area.CountUnassignedLocations;
            _counts[null, false] = area.CountNonemptyLocations;
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

        [Display(Name = "Picking area") ]
        public string AreaId { get; set; }
    }
}