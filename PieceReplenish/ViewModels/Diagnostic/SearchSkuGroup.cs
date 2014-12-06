using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.PieceReplenish.ViewModels.Diagnostic
{
    /// <summary>
    /// This is an immutable class
    /// </summary>
    public class SearchSkuGroup : IComparable<SearchSkuGroup>, IEquatable<SearchSkuGroup>
    {
        private readonly string _vwhId;
        private readonly string _buildingId;
        private int? _restockCartonCount;
        private int? _restockCartonPieces;
        private int? _piecesRequiredAtLocation;
        private int _pullableCartonCount;
        private readonly string _pullAreaId;
        private string _pickAreaId;
        private string _shortName;
        private readonly string _restockAreaId;
        private int? _locationCapacity;
        private int? _locationCount;
        private int? _piecesAtLocations;

        public SearchSkuGroup(string buildingId, string vwhId, string restockAreaId, string replenishAreaId)
        {
            _vwhId = vwhId;
            _buildingId = buildingId;
            _restockAreaId = restockAreaId;
            _pullAreaId = replenishAreaId;
        }


        public void UpdateRestockTotals(IList<DiagnosticCartonModel> restockCartons)
        {
            _restockCartonCount = restockCartons.Count;
            _restockCartonPieces = restockCartons.Sum(p => p.Quantity);
        }
        public void UpdateRequirementDetails(IList<SkuRequirementModel> requirements)
        {
            _piecesRequiredAtLocation = requirements.Sum(p => p.PiecesRequiredAtLocation);
            if (requirements != null)
            {
                _pickAreaId = requirements.First().PickAreaId;
                _shortName = requirements.First().ShortName;
                _locationCapacity = requirements.Sum(p => p.LocationCapacity);
                _locationCount = requirements.Count();
                _piecesAtLocations = requirements.Sum(p => p.PiecesAtLocation);
            }
        }

        [DisplayFormat(DataFormatString = "({0:N0})")]
        public int? PiecesAtLocations
        {
            get
            {
                return _piecesAtLocations;
            }
        }

        [DisplayFormat(DataFormatString = "Location Capacity of {0:N0} locations", NullDisplayText = "Location Capacity")]
        public int? LocationCount
        {
            get
            {
                return _locationCount;
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}", NullDisplayText = "Not assigned to any location")]
        public int? LocationCapacity
        {
            get
            {
                return _locationCapacity;
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesToPull
        {
            get
            {
                return (_piecesRequiredAtLocation ?? 0) - (_restockCartonPieces ?? 0) < 0 ? (int?)null : ((_piecesRequiredAtLocation ?? 0) - (_restockCartonPieces ?? 0));
            }
        }

        /// <summary>
        /// Total number of cartons in RST awaiting restock for this group
        /// </summary>
        [Display(Name = "Cartons in Restock")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? RestockCartonCount
        {
            get
            {
                return _restockCartonCount;
            }
        }

        /// <summary>
        /// Total number of pieces in all cartons which are awaiting restock for this group
        /// </summary>
        [Display(Name = "Restock Pieces")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? RestockCartonPieces
        {
            get
            {
                return _restockCartonPieces;
            }
        }

        /// <summary>
        /// How many pieces are required to fulfill the location of current building for this group
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesRequiredAtLocation
        {
            get
            {
                return _piecesRequiredAtLocation;
            }
        }

        public void UpdatePullableCartonCount(IList<DiagnosticCartonModel> cartonsToPull)
        {
            foreach (var carton in cartonsToPull)
            {
                if (carton.CanPullCarton && ((_piecesRequiredAtLocation ?? 0) - (_restockCartonPieces ?? 0) >= carton.CumPieces))
                {
                    _pullableCartonCount++;
                }
            }
        }

        /// <summary>
        /// How many cartons can be pulled from carton area to fulfill the location of current building for this group
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PullableCartonCount
        {
            get
            {
                return _pullableCartonCount;
            }
        }

        public string VwhId
        {
            get
            {
                return _vwhId;
            }
        }

        public string BuildingId
        {
            get
            {
                return _buildingId;
            }
        }

        /// <summary>
        /// Pull area id for this group
        /// </summary>
        public string ReplenishAreaId
        {
            get { return _pullAreaId; }
        }

        /// <summary>
        /// Restock area id for this group
        /// </summary>
        public string RestockAreaId
        {
            get { return _restockAreaId; }
        }

        public string PickAreaId
        {
            get { return _pickAreaId; }
        }

        /// <summary>
        /// Area short name
        /// </summary>
        public string ShortName
        {
            get 
            {
                return _shortName; 
            }
        }
        public string HtmlId
        {
            get
            {
                return string.Format("{0}_{1}", this.VwhId, this.BuildingId);
            }
        }

        public bool Equals(SearchSkuGroup other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Null building matches every building
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(SearchSkuGroup other)
        {
            var ret = string.IsNullOrWhiteSpace(_buildingId) || string.IsNullOrWhiteSpace(other._buildingId) ? 0 : _buildingId.CompareTo(other._buildingId);
            if (ret == 0)
            {
                ret = _vwhId.CompareTo(other._vwhId);
            }
            return ret;
        }

        public override int GetHashCode()
        {
            return _buildingId.GetHashCode();
        }
    }
}