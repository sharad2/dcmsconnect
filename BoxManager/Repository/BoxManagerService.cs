using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

//Reviewed By: Shiva Pandey and Deepak Bhatt on  11 June 2012
namespace DcmsMobile.BoxManager.Repository
{
    public enum BoxManagerServiceErrorCode
    {
        Unknown,

        /// <summary>
        /// An attempt was made to merge the pallet with itself
        /// </summary>
        MergingPalletWithSelf,

        /// <summary>
        /// Boxes on Pallet belong to multiple customers. The diagnostic data contains a comma separated list of customers.
        /// </summary>
        MultipleCustomerPallet,

        /// <summary>
        /// Boxes on Pallet belong to multiple buckets. The diagnostic data contains a comma separated list of bucket.
        /// </summary>
        MultipleBucketPallet,

        /// <summary>
        /// Boxes on Pallet belong to multiple DCs. The diagnostic data contains a comma separated list of DCs.
        /// </summary>
        MultipleDcPallet,

        /// <summary>
        /// Boxes on Pallet belong to multiple POs. The diagnostic data contains a comma separated list of POs.
        /// </summary>
        MultiplePoPallet,

        /// <summary>
        /// When user tries to merge two empty pallets.
        /// </summary>
        BothPalletEmpty,

        /// <summary>
        /// Boxes on Pallet belong to multiple Area. The diagnostic data contains a comma separated list of Area.
        /// </summary>
        MultipleAreaPallet,

        /// <summary>
        /// Boxes on Pallet belong to multiple Locations. The diagnostic data contains a comma separated list of Locations.
        /// </summary>
        MultipleLocationPallet,

        /// <summary>
        /// When scanned location does not exists in the system.
        /// </summary>
        InvalidLocation
    }

    /// <summary>
    /// Sounds to confirm scans. Used by home controller.
    /// </summary>
    public enum Sound
    {
        /// <summary>
        /// Error Sound when something catastrophic happens
        /// </summary>
        Error = 'E',

        /// <summary>
        /// When something unusual happens
        /// </summary>
        Warning = 'W',

        /// <summary>
        /// Nothing happened.
        /// </summary>
        None = '\0'
    }

    /// <summary>
    /// This class provides the handling of BoxManager program exceptions.
    /// </summary>
    public sealed class BoxManagerServiceException : Exception
    {
        public BoxManagerServiceException()
        {
        }

        private readonly BoxManagerServiceErrorCode _errorCode;
        public BoxManagerServiceException(BoxManagerServiceErrorCode errorCode)
        {
            _errorCode = errorCode;
        }

        public BoxManagerServiceException(BoxManagerServiceErrorCode errorCode, string diagnostic)
        {
            _errorCode = errorCode;
            this.Data.Add("Data", diagnostic);
        }

        public BoxManagerServiceErrorCode ErrorCode
        {
            get
            {
                return _errorCode;
            }
        }
    }



    /// <summary>
    /// Pallet limit is cached in static variable. Also cached in app memory with absolute expiration.
    /// </summary>
    public class BoxManagerService : IDisposable
    {

        #region Intialization

        private readonly BoxManagerRepository _repos;

        /// <summary>
        /// For unit tests. 
        /// </summary>
        public BoxManagerService(BoxManagerRepository repos)
        {
            _repos = repos;
        }

        /// <summary>
        /// Used to store destination area of intransit cartons until they are received
        /// </summary>      
        public BoxManagerService(TraceContext ctx, string connectString, string userName, string clientInfo, string moduleName)
        {
            _repos = new BoxManagerRepository(ctx, connectString, userName, clientInfo, moduleName);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }

        #endregion

        public int QueryCount
        {
            get
            {
                return _repos.QueryCount;
            }
        }

        #region Application Cache
        private const string APPKEY_SORTCRITERIA = "BoxManagerService_SortCriteria";
        private const string APPKEY_QUAL_COUNT = "BoxManagerService_QualCount";
        private const string APPKEY_QUAL_COUNT_VAS = "BoxManagerService_QualBoxCount_Vas";
        private const string APPKEY_BADPITCHAREA = "BoxManagerService_BadPitchArea";

        /// <summary>
        /// Key is customer id, value is SortCriteria
        /// </summary>
        private ConcurrentDictionary<string, SortCriteria> CachedSortCriteria
        {
            get
            {
                var sortCriteria = MemoryCache.Default[APPKEY_SORTCRITERIA] as ConcurrentDictionary<string, SortCriteria>;
                if (sortCriteria == null)
                {
                    sortCriteria = new ConcurrentDictionary<string, SortCriteria>();
                    MemoryCache.Default.Add(APPKEY_SORTCRITERIA, sortCriteria, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(30)
                    });
                }
                return sortCriteria;
            }
        }

        /// <summary>
        /// Encasulates the values used for qualification criteria
        /// </summary>
        private class QualificationCriteria : IEquatable<QualificationCriteria>
        {
            public string CustomerId { private get; set; }
            public string PoId { private get; set; }
            public string CustomerDcId { private get; set; }
            public int? BucketId { private get; set; }

            public bool Equals(QualificationCriteria other)
            {
                if (other == null)
                {
                    return false;
                }
                return this.CustomerId == other.CustomerId && this.CustomerDcId == other.CustomerDcId && this.PoId == other.PoId && this.BucketId == other.BucketId;
            }

            public override bool Equals(object obj)
            {
                var qual = obj as QualificationCriteria;
                if (qual == null)
                {
                    return false;
                }
                return Equals(qual);
            }

            public override int GetHashCode()
            {
                // By returning the same hash code for all instances, we are focring the call to Equals()
                return 0;
            }
        }

        /// <summary>
        /// Caches the qualifying box counts per sort criteria.
        /// </summary>
        private ConcurrentDictionary<QualificationCriteria, int> CachedQualifyingBoxes
        {
            get
            {
                var qualCounts = MemoryCache.Default[APPKEY_QUAL_COUNT] as ConcurrentDictionary<QualificationCriteria, int>;
                if (qualCounts == null)
                {
                    qualCounts = new ConcurrentDictionary<QualificationCriteria, int>();
                    MemoryCache.Default.Add(APPKEY_QUAL_COUNT, qualCounts, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(15)
                    });
                }
                return qualCounts;
            }
        }

        /// <summary>
        /// Caches the qualifying box counts per sort criteria.
        /// </summary>
        private ConcurrentDictionary<string, int> CachedQualifyingBoxesForVas
        {
            get
            {
                var count = MemoryCache.Default[APPKEY_QUAL_COUNT_VAS] as ConcurrentDictionary<string, int>;
                if (count == null)
                {
                    count = new ConcurrentDictionary<string, int>();
                    MemoryCache.Default.Add(APPKEY_QUAL_COUNT_VAS, count, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(15)
                    });
                }
                return count;
            }
        }

        /// <summary>
        /// Cache bad pitch area name
        /// </summary>
        private ConcurrentDictionary<string, string> AreaCache
        {
            get
            {
                var cache = MemoryCache.Default[APPKEY_BADPITCHAREA] as ConcurrentDictionary<string, string>;
                if (cache == null)
                {
                    cache = new ConcurrentDictionary<string, string>();
                    // Caching for an hour
                    MemoryCache.Default.Add(APPKEY_BADPITCHAREA, cache, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(60)
                    });
                }
                return cache;
            }
        }


        #endregion

        IEnumerable<Box> _palletBoxes;

        /// <summary>
        /// Returns all boxes of the passed pallet
        /// Sharad 6 Jul 2012. Remember the result for the session so that this function can be called multiple times without re-executing database query.
        /// This cache is discarded when a box is put on a pallet.
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns>List of the boxes</returns>
        public IEnumerable<Box> GetBoxesOfPallet(string palletId)
        {
            if (string.IsNullOrWhiteSpace(palletId))
            {
                throw new ArgumentNullException("palletId");
            }

            if (_palletBoxes != null && _palletBoxes.Any() && _palletBoxes.First().PalletId == palletId)
            {
                // The same pallet is being required. Return the same result.
                return _palletBoxes;

            }

            _palletBoxes = _repos.GetBoxes(palletId, null);
            return _palletBoxes;
        }

        private static decimal? __palletVolumeLimit;

        /// <summary>
        /// Following function is for retrieving the pallet volume limit.
        /// The pallet limit is cached so that we do not have to query each time
        /// Factory default of 7 cubic feet is assumed if not specified in the database
        /// </summary>
        /// <returns>pallet volume limit</returns> 
        public decimal GetPalletVolumeLimit()
        {
            if (__palletVolumeLimit == null)
            {
                var limit = _repos.GetPalletVolumeLimit();
                // Factory default of 7 if not specified in the database
                __palletVolumeLimit = limit ?? 7;
            }
            return __palletVolumeLimit.Value;
        }

        /// <summary>
        /// Places the passed box on the passed pallet.
        /// </summary>
        /// <param name="palletId">The pallet to place the box on. If null, a new pallet id is created.</param>
        /// <param name="ucc128Id">The box to place on the pallet.</param>
        /// <param name="isVasUi"> </param>
        /// <returns>The pallet on which the box was placed. Useful when a new pallet id is created.</returns>
        /// <remarks>
        /// Decrements the cached qualifying box count by 1 for the criteria of the passed box
        /// </remarks>
        public string PutBoxOnPallet(string palletId, string ucc128Id, bool isVasUi)
        {
            if (string.IsNullOrWhiteSpace(ucc128Id))
            {
                throw new ArgumentNullException("ucc128Id");
            }
            if (string.IsNullOrWhiteSpace(palletId))
            {
                palletId = _repos.GetTemporaryPalletId();
            }
            _repos.PutBoxOnPallet(ucc128Id, palletId, palletId.StartsWith("T"), isVasUi);

            // Sharad 6 Jul 2012. The pallet boxes cache is now stale. Requery it
            _palletBoxes = _repos.GetBoxes(palletId, null);
            var box = _palletBoxes.FirstOrDefault();
            if (box == null)
            {
                // Should not happen
                return palletId;
            }
            int oldCount;
            if (isVasUi)
            {
                if (CachedQualifyingBoxesForVas.TryGetValue(box.CustomerId, out oldCount) && oldCount > 0)
                {
                    // Now there is one less qualifying box for VAS
                    CachedQualifyingBoxesForVas[box.CustomerId] = oldCount - 1;
                }
            }
            else
            {
                var criteria = GetSortCriteria(box.CustomerId, false);
                var qual = new QualificationCriteria
                {
                    BucketId = criteria.HasFlag(SortCriteria.AllowBucketMixing) ? null : box.BucketId,
                    CustomerDcId = criteria.HasFlag(SortCriteria.AllowCustomerDcMixing) ? null : box.CustomerDcId,
                    CustomerId = box.CustomerId,
                    PoId = criteria.HasFlag(SortCriteria.AllowPoMixing) ? null : box.PoId
                };
                if (CachedQualifyingBoxes.TryGetValue(qual, out oldCount) && oldCount > 0)//MBisht (6 July 2012): If the count is already 0 then no need to further decrementing the count as it will become negative. 
                {
                    // Now there is one less qualifying box for STP
                    CachedQualifyingBoxes[qual] = oldCount - 1;
                }
            }
            return palletId;
        }

        /// <summary>
        /// This will update the location as well as the area of the box
        /// </summary>
        /// <param name="palletId"></param>
        /// <param name="locationId"></param>
        /// <returns>This is a void function</returns>
        /// <remarks>This function will update the location of the pallet. 
        /// On updating location, area of the each box in pallet will
        /// get changed to the area of passed location.
        /// </remarks>
        public void UpdatePalletLocation(string palletId, string locationId)
        {
            if (string.IsNullOrWhiteSpace(palletId))
            {
                throw new ArgumentNullException("palletId");
            }
            if (string.IsNullOrWhiteSpace(locationId))
            {
                throw new ArgumentNullException("locationId");
            }

            _repos.UpdatePalletLocation(palletId, locationId);
        }

        /// <summary>
        /// This function ensures whether the passed location is valid or not.
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns>This is a void function</returns>
        public void EnsureLocationIsValid(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                throw new ArgumentNullException("locationId");
            }
            if (!_repos.IsLocationValid(locationId))
            {
                throw new BoxManagerServiceException(BoxManagerServiceErrorCode.InvalidLocation);
            }
        }

        /// <summary>
        /// Returns the details of the passed box. 
        /// </summary>
        /// <param name="ucc128Id"></param>
        /// <param name="isVasUI"> </param>
        /// <returns>Details of the box</returns>
        public Box GetBox(string ucc128Id, bool isVasUI)
        {
            return _repos.GetBoxes(null, ucc128Id, isVasUI).FirstOrDefault();
        }

        /// <summary>
        /// MBisht:-
        /// This function is for retreiving the good pitch area from table iaconfig against iaconfig id $GOODPITCH
        /// The retreived good pitch area will be kept in the cache for 60  minutes.
        /// </summary>
        /// <returns>Good pitch area</returns>
        public string GetBadPitchArea()
        {
            string strBadPitchArea;
            this.AreaCache.TryGetValue(APPKEY_BADPITCHAREA, out strBadPitchArea);
            if (string.IsNullOrWhiteSpace(strBadPitchArea))
            {
                strBadPitchArea = _repos.GetBadPitchArea();
                AreaCache.TryAdd(APPKEY_BADPITCHAREA, strBadPitchArea);
            }
            return strBadPitchArea;
        }

        /// <summary>
        /// Places the passed source pallet boxes on the passed destination pallet.
        /// </summary>
        /// <param name="sourcePalletId">Pallet on which user has sorted the criteria boxes</param>
        /// <param name="destPalletId">The pallet on which the boxes of source pallet are to be placed. </param>
        /// <param name="isVasUi"> </param>
        /// <returns> void function.</returns>
        /// <return>This is a void function</return>
        public void MergePallets(string sourcePalletId, string destPalletId, bool isVasUi)
        {
            if (string.IsNullOrWhiteSpace(sourcePalletId))
            {
                throw new ArgumentNullException("sourcePalletId");
            }
            if (string.IsNullOrWhiteSpace(destPalletId))
            {
                throw new ArgumentNullException("destPalletId");
            }

            var boxesStagingPallet = this.GetBoxesOfPallet(sourcePalletId);

            using (var trans = _repos.BeginTransaction())
            {
                foreach (var box in boxesStagingPallet)
                {
                    PutBoxOnPallet(destPalletId, box.Ucc128Id, isVasUi);
                }
                trans.Commit();
            }
        }

        /// <summary>
        /// This function ensures that passed pallets can be merged.It also ensures that both pallet are not empty
        /// and not same.If destination Pallet is placed on multiple area or multiple location then gives exception.
        /// If source area pallet has boxes of multiple area, location we do not consider it as a problem because 
        /// after merging, the source pallet boxes will be updated with the destination area and location. 
        /// </summary>
        /// <param name="sourcePalletId"></param>
        /// <param name="destPalletId"></param>
        /// <param name="isVasUi"> </param>
        /// <remarks>check criteria of both pallet and then decide whether the pallets can be merged together or not
        /// It also calculates that what would be the percentage of resulting pallet after merging. This value is returned 
        /// in an out parameter.
        /// </remarks>
        public decimal EnsureMergePallet(string sourcePalletId, string destPalletId, bool isVasUi)
        {
            if (string.IsNullOrWhiteSpace(sourcePalletId))
            {
                throw new ArgumentNullException("sourcePalletId");
            }
            if (string.IsNullOrWhiteSpace(destPalletId))
            {
                throw new ArgumentNullException("destPalletId");
            }
            if (sourcePalletId == destPalletId)
            {
                throw new BoxManagerServiceException(BoxManagerServiceErrorCode.MergingPalletWithSelf);
            }
            var boxesNewPallet = this.GetBoxesOfPallet(destPalletId).ToList();
            var boxesStagingPallet = this.GetBoxesOfPallet(sourcePalletId);

            var areas = boxesNewPallet.Select(p => p.IaId).Distinct().ToArray();
            if (areas.Length > 1)
            {
                // Impure pallet more than one area.
                throw new BoxManagerServiceException(BoxManagerServiceErrorCode.MultipleAreaPallet, string.Join(", ", areas));
            }

            var location = boxesNewPallet.Select(p => p.LocationId).Distinct().ToArray();
            if (location.Length > 1)
            {
                // Impure pallet more than one location.
                throw new BoxManagerServiceException(BoxManagerServiceErrorCode.MultipleLocationPallet, string.Join(", ", location));
            }


            // If user scan both empty pallet.
            var boxesOnBothPallet = boxesNewPallet.Concat(boxesStagingPallet).ToList();
            if (!boxesOnBothPallet.Any())
            {
                throw new BoxManagerServiceException(BoxManagerServiceErrorCode.BothPalletEmpty);
            }

            this.EnsureCriteriaPure(boxesOnBothPallet, isVasUi);
            return boxesOnBothPallet.Sum(p => p.Volume);
        }

        /// <summary>
        /// Returns the number of under process unpalletized boxes for the passed criteria
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="poId"></param>
        /// <param name="customerDcId"></param>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        /// <remarks>
        /// This count is cached per criteria for performance reasons.
        /// </remarks>
        public int GetQualifyingBoxCount(string customerId, string poId, string customerDcId, int? bucketId)
        {
            int count;
            var qual = new QualificationCriteria
            {
                CustomerId = customerId,
                PoId = poId,
                CustomerDcId = customerDcId,
                BucketId = bucketId

            };
            var b = CachedQualifyingBoxes.TryGetValue(qual, out count);
            if (b)
            {
                return count;
            }
            count = _repos.GetQualifyingBoxCount(customerId, poId, customerDcId, bucketId, false, false);
            CachedQualifyingBoxes.TryAdd(qual, count);
            return count;
        }

        /// <summary>
        /// Returns the number of under process non-palletized boxes for VAS for the passed customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        /// <remarks>
        /// This count is cached per criteria for performance reasons.
        /// </remarks>
        public int GetQualifyingBoxCountForVas(string customerId)
        {
            int count;
            var b = CachedQualifyingBoxesForVas.TryGetValue(customerId, out count);
            if (b)
            {
                return count;
            }
            count = _repos.GetQualifyingBoxCount(customerId, null, null, null, false, true);
            CachedQualifyingBoxesForVas.TryAdd(customerId, count);
            return count;
        }

        /// <summary>
        /// Retrieves the count of verified boxes of passed customer criteria.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="poId"></param>
        /// <param name="customerDcId"></param>
        /// <param name="bucketId"></param>
        /// <returns>Returns the count of verified boxes</returns>
        public int GetVerifiedBoxes(string customerId, string poId, string customerDcId, int? bucketId)
        {
            return _repos.GetQualifyingBoxCount(customerId, poId, customerDcId, bucketId, true, false);
        }

        /// <summary>
        /// This function calculates the sorting criteria and also ensures that the list 
        /// of passed boxes belongs to same criteria. 
        /// </summary>
        /// <param name="boxes"></param>
        /// <param name="isVasUi"> </param>
        /// <returns>Sorting criteria</returns>
        /// <exception cref="BoxManagerServiceException">Boxes do not belong to the same criteria</exception>
        public SortCriteria EnsureCriteriaPure(IEnumerable<Box> boxes, bool isVasUi)
        {
            var customers = boxes.Select(p => p.CustomerId).Distinct().ToArray();
            if (customers.Length != 1)
            {
                // Impure pallet more than one customer.
                throw new BoxManagerServiceException(BoxManagerServiceErrorCode.MultipleCustomerPallet, string.Join(", ", customers));
            }
            var sortCriteria = this.GetSortCriteria(customers[0], isVasUi);
            var buckets = boxes.Select(p => p.BucketId).Distinct().ToArray();
            if (!sortCriteria.HasFlag(SortCriteria.AllowBucketMixing) && buckets.Length != 1)
            {
                // Impure pallet more than one bucket.
                throw new BoxManagerServiceException(BoxManagerServiceErrorCode.MultipleBucketPallet, string.Join(", ", buckets));
            }
            var customerDc = boxes.Select(p => p.CustomerDcId).Distinct().ToArray();
            if (!sortCriteria.HasFlag(SortCriteria.AllowCustomerDcMixing) && customerDc.Length != 1)
            {
                // Impure pallet more than one DC.
                throw new BoxManagerServiceException(BoxManagerServiceErrorCode.MultipleDcPallet, string.Join(", ", customerDc));
            }
            var customerPo = boxes.Select(p => p.PoId).Distinct().ToArray();
            if (!sortCriteria.HasFlag(SortCriteria.AllowPoMixing) && customerPo.Length != 1)
            {
                // Impure pallet more than one PO.
                throw new BoxManagerServiceException(BoxManagerServiceErrorCode.MultiplePoPallet, string.Join(", ", customerPo));
            }

            return sortCriteria;
        }

        /// <summary>
        /// Retrieves the list of pallet that are of the passed criteria and sufficient capacity to accommodate the cartons of passed pallet.  
        /// </summary>
        /// <param name="palletId"></param>
        /// <param name="customerId"></param>
        /// <param name="bucketId"></param>
        /// <param name="poId"></param>
        /// <param name="customerDcId"></param>
        /// <param name="isVasPalletSuggestion">if true, returns only those pallets which having boxes for VAS</param>
        /// <returns>This function will retrieve a list of pallets</returns>
        public IEnumerable<Pallet> SuggestPallets(string palletId, string customerId, int? bucketId, string poId, string customerDcId, bool isVasPalletSuggestion = false)
        {
            var boxes = GetBoxesOfPallet(palletId);
            var totalBoxVolume = boxes.Sum(p => p.Volume);
            var palletVolumeLimit = GetPalletVolumeLimit();
            var pallets = Enumerable.Empty<Pallet>();
            if (totalBoxVolume < palletVolumeLimit)
            {
                var effectivePalletVolumeLimit = palletVolumeLimit - totalBoxVolume;
                pallets = _repos.SuggestPalletOrLocation(customerId, bucketId, poId, customerDcId, effectivePalletVolumeLimit, true, null, isVasPalletSuggestion);
            }
            return pallets;
        }

        /// <summary>
        /// Suggest location of other area.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="bucketId"></param>
        /// <param name="poId"></param>
        /// <param name="customerDcId"></param>
        /// <param name="palletArea"></param>
        /// <returns>
        /// list of location.
        /// </returns>
        public IEnumerable<Pallet> SuggestLocation(string customerId, int? bucketId, string poId, string customerDcId, string palletArea)
        {
            return _repos.SuggestPalletOrLocation(customerId, bucketId, poId, customerDcId, null, false, palletArea);
        }

        /// <summary>
        /// Gets the sort criteria of passed customer. We check the cache first, if the value is found there 
        /// we return it, otherwise we query the database and add the value to cache.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="isVasUi"> </param>
        /// <returns></returns>
        private SortCriteria GetSortCriteria(string customerId, bool isVasUi)
        {
            SortCriteria sortCriteria;
            if (isVasUi)
            {
                sortCriteria = SortCriteria.AllowBucketMixing | SortCriteria.AllowCustomerDcMixing | SortCriteria.AllowPoMixing;
            }
            else if (!CachedSortCriteria.TryGetValue(customerId, out sortCriteria))
            {
                sortCriteria = _repos.GetSortCriteria(customerId);
                CachedSortCriteria.TryAdd(customerId, sortCriteria);
            }
            return sortCriteria;
        }

        /// <summary>
        /// Program put all boxes of pallet in suspense.
        /// </summary>
        /// <param name="palletId"></param>
        public void PutBoxOfPalletInSuspense(string palletId)
        {
            _repos.PutBoxOfPalletInSuspence(palletId);
        }

        /// <summary>
        /// Removes suspense date from the passed box. If the passed box is not on the pallet then it puts the box on pallet if the criteria's match.   
        /// </summary>
        /// <param name="palletId"></param>
        /// <param name="ucc128Id"></param>
        /// <param name="isVasUi"> </param>
        public void TryUpdateBox(string palletId, string ucc128Id, bool isVasUi)
        {
            if (!_repos.RemoveBoxFromSuspense(palletId, ucc128Id))
            {
                var boxes = _repos.GetBoxes(null, ucc128Id);
                if (boxes != null && !boxes.Any())
                {
                    throw new BoxManagerServiceException(BoxManagerServiceErrorCode.Unknown);
                }
                // Check Box Criteria match pallet criteria.
                var boxOfPallet = this.GetBoxesOfPallet(palletId).Concat(boxes);
                EnsureCriteriaPure(boxOfPallet, isVasUi);
                this.PutBoxOnPallet(palletId, ucc128Id, isVasUi);
            }
        }

        /// <summary>
        /// Removes the passed box from pallet.
        /// </summary>
        /// <param name="ucc128Id"></param>
        public void RemovePalletFromBox(string ucc128Id)
        {
            _repos.RemovePalletFromBox(ucc128Id);
        }

        /// <summary>
        /// Returns count of boxes which are not in suspense.
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns>
        /// count of boxes.
        /// </returns>
        public int GetValidBoxesCount(string palletId)
        {
            return _repos.GetValidBoxesCount(palletId);
        }

        public void MarkVasComplete(string ucc128Id)
        {
            _repos.MarkVasComplete(ucc128Id);
        }
    }
}



