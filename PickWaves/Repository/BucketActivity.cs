using System;
using System.Collections.ObjectModel;
using DcmsMobile.PickWaves.Helpers;

namespace DcmsMobile.PickWaves.Repository
{
    /// <summary>
    /// The goal of an activity it transition the state of each box to verified. Other possible box states are
    /// unverified and cancelled.
    /// 
    /// A box has two types of pieces: Expected and curent.
    /// </summary>
    internal class BucketActivity
    {
        private readonly BoxStats _stats;
        public BucketActivity()
        {
            _stats = new BoxStats();
        }

        public BucketActivityType ActivityType { get; set; }

        public BoxStats Stats
        {
            get
            {
                return _stats;
            }
        }

        public DateTimeOffset? MaxEndDate { get; set; }

        public DateTimeOffset? MinEndDate { get; set; }

        private InventoryArea _area;
        public InventoryArea Area
        {
            get
            {
                return _area ?? (_area = new InventoryArea());
            }
            set { _area = value; }
        }

    }

    /// <summary>
    /// This collection always has activities for pitching and pulling
    /// </summary>
    internal class BucketActivityCollection : KeyedCollection<BucketActivityType, BucketActivity>
    {

        public BucketActivityCollection()
        {
            this.Add(new BucketActivity
            {
                ActivityType = BucketActivityType.Pitching
            });

            this.Add(new BucketActivity
            {
                ActivityType = BucketActivityType.Pulling
            });
        }

        protected override BucketActivityType GetKeyForItem(BucketActivity item)
        {
            return item.ActivityType;
        }
    }

}
