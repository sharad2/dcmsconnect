using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DcmsMobile.PickWaves.Repository;
using System;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    /// <summary>
    /// IEquatable implementation allows us to treat areas as same if there id is same. This makes it possible to use Distinct() for InventoryAreaModel
    /// </summary>
    public class InventoryAreaModel
    {
        protected InventoryAreaModel()
        {

        }

        internal InventoryAreaModel(InventoryArea area)
        {
            AreaId = area.AreaId;
            ShortName = area.ShortName;
            BuildingId = area.BuildingId;
            Description = area.Description;
            ReplenishAreaId = area.ReplenishAreaId;
        }

        [DisplayFormat(NullDisplayText = "(Not Specified)")]
        public string AreaId { get; set; }

        private string _shortName;

        /// <summary>
        /// If short name is not available, we display area id
        /// </summary>
        [DisplayFormat(NullDisplayText = "(Not Specified)")]
        public string ShortName
        {
            get
            {
                if (string.IsNullOrEmpty(this.AreaId))
                {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(_shortName))
                {
                    return this.AreaId;
                }
                return _shortName;
            }
            set { _shortName = value; }
        }

        private string _description;

        [DisplayFormat(NullDisplayText = "(Not Specified)")]
        public string Description
        {
            get
            {
                if (string.IsNullOrEmpty(this.AreaId))
                {
                    return string.Empty;
                }
                return _description;
            }
            set { _description = value; }
        }

        [DisplayFormat(NullDisplayText = "(Not Specified)")]
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this.AreaId))
                {
                    return string.Empty;
                }
                return string.Format("{0}: {1}", this.ShortName, this.Description);
            }
        }

        public string BuildingId { get; set; }

        public string BuildingName { get; set; }

        public string ReplenishAreaId { get; set; }
    }

    public class InventoryAreaModelComparer : IEqualityComparer<InventoryAreaModel>
    {
        private static IEqualityComparer<InventoryAreaModel> __instance;
        public static IEqualityComparer<InventoryAreaModel> Instance
        {
            get
            {
                if (__instance == null)
                {
                    __instance = new InventoryAreaModelComparer();
                }
                return __instance;
            }
        }

        /// <summary>
        /// Constructor is private
        /// </summary>
        private InventoryAreaModelComparer()
        {

        }

        public bool Equals(InventoryAreaModel x, InventoryAreaModel y)
        {
            return x.AreaId == y.AreaId; 
        }

        public int GetHashCode(InventoryAreaModel obj)
        {
            return obj.AreaId.GetHashCode();
        }
    }
}
