using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DcmsMobile.PickWaves.Helpers;

namespace DcmsMobile.PickWaves.Repository.ManageWaves
{
    public class ManageWavesService : PickWaveServiceBase<ManageWavesRepository>
    {
        #region Intialization

        public ManageWavesService(TraceContext trace, string userName, string clientInfo)
        {
            _repos = new ManageWavesRepository(trace, userName, clientInfo);
        }
        #endregion

        /// <summary>
        /// All parameters are optional. If none are specified, all buckets will be retrieved 
        /// which is definitely what you will not want.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal IEnumerable<Bucket> GetBuckets(string customerId, ProgressStage state)
        {
            return _repos.GetBuckets(null, customerId, state);
        }

        /// <summary>
        /// SKU list of passed bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="stateFilter"> </param>
        /// <param name="activityFilter"> </param>
        /// <returns></returns>
        public IEnumerable<BucketSku> GetBucketSkuList(int bucketId, BoxState stateFilter, BucketActivityType activityFilter)
        {
            return _repos.GetBucketSkuList(bucketId, stateFilter, activityFilter);
        }

        /// <summary>
        /// Pickslip of passed bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        public IEnumerable<Pickslip> GetBucketPickslip(int bucketId)
        {
            return _repos.GetBucketPickslip(bucketId);
        }

        /// <summary>
        /// Boxes of passed bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        public IEnumerable<Box> GetBucketBoxes(int bucketId, BoxState stateFilter, BucketActivityType activityFilter)
        {
            return _repos.GetBucketBoxes(bucketId, stateFilter, activityFilter);
        }

        /// <summary>
        /// Edit bucket property. Error if you attempt to edit an unfrozen wave
        /// </summary>
        /// <param name="bucket"></param>
        internal Bucket EditWave(Bucket bucket, EditBucketFlags flags, Bucket bucketOld)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }
            using (var trans = _repos.BeginTransaction())
            {
                var bucketCurrent = _repos.GetLockedBucket(bucket.BucketId);
                if (bucketCurrent == null)
                {
                    throw new ValidationException("Invalid Pick Wave" + bucket.BucketId.ToString());
                }
                if (!bucketCurrent.IsFrozen)
                {
                    throw new ValidationException("Only frozen Waves can be edited");
                }
                if (!bucketCurrent.Equals(bucketOld))
                {
                    throw new ValidationException("Cannot edit. Bucket has been modified by someone else.");
                }
                var updatedWave = _repos.EditWave(bucket, flags);
                trans.Commit();
                return updatedWave;
            }
        }

        public void RemovePickslipFromBucket(long pickslipId, int bucketId)
        {
            _repos.RemovePickslipFromBucket(pickslipId, bucketId);
        }

        /// <summary>
        /// If bucket is already freeze then do nothing.
        /// Otherwise when bucket is freeze,delete the unpicked boxes of bucket.
        /// and if bucket is unfreeze,create the boxes of unpicked pieces.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="freeze"></param>
        public Bucket FreezeWave(int bucketId, bool freeze)
        {
            using (var trans = _repos.BeginTransaction())
            {
                var bucket = _repos.GetLockedBucket(bucketId);
                if (bucket == null)
                {
                    throw new ValidationException("Invalid Pick Wave " + bucketId.ToString());
                }

                var pullArea = bucket.Activities.Single(p => p.ActivityType == BucketActivityType.Pulling).Area.AreaId;
                var pitchArea = bucket.Activities.Single(p => p.ActivityType == BucketActivityType.Pitching).Area.AreaId;
                if (string.IsNullOrWhiteSpace(pullArea) && string.IsNullOrWhiteSpace(pitchArea))
                {
                    throw new ValidationException("Please select at least one area for pulling and/ pitching.");
                }

                if (bucket.IsFrozen == freeze)
                {
                    // Nothing to do
                    return bucket;
                }
                if (freeze)
                {
                    // Delete boxes
                    _repos.DeleteBoxes(bucketId);
                }
                else
                {
                    // Create Boxes
                    _repos.CreateBoxes(bucketId);
                }
                _repos.SetFreezeStatus(bucketId, freeze);
                trans.Commit();
                return bucket;
            }
        }

        /// <summary>
        /// Increase priority by 1.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        public Bucket IncrementPriority(int bucketId)
        {
            return _repos.EditWave(new Bucket
            {
                BucketId = bucketId,
                PriorityId = 1
            }, EditBucketFlags.PriorityDelta);
        }

        /// <summary>
        /// Decrease priority by 1.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        public Bucket DecrementPriority(int bucketId)
        {
            return _repos.EditWave(new Bucket
            {
                BucketId = bucketId,
                PriorityId = -1
            }, EditBucketFlags.PriorityDelta);
        }

        /// <summary>
        /// Get area list for bucket sku.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        public IEnumerable<BucketArea> GetBucketAreas(int bucketId)
        {
            return _repos.GetBucketAreas(bucketId);
        }
    }
}

/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/
