using System;

namespace DcmsMobile.BoxManager.Repository
{
    /// <summary>
    /// This entity represents the rules which control placement of boxes on pallets. For all pallets, all non null values for each box must match.
    /// </summary>
    [Flags]
    public enum SortCriteria
    {
        NoMixing = 0x0,

        AllowBucketMixing = 0x1,

        AllowPoMixing = 0x2,

        AllowCustomerDcMixing = 0x4
    }
}