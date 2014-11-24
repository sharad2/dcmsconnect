﻿namespace DcmsMobile.PickWaves.Repository.ManageWaves
{
    internal class BucketArea : InventoryArea
    {
        /// <summary>
        /// Number of ordered SKU which can be found in this area
        /// </summary>
        public int? CountSku { get; set; }

        public int? CountOrderedSku { get; set; }
    }
}