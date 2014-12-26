using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    internal class ManageWavesService : PickWaveServiceBase<ManageWavesRepository>
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
        internal IEnumerable<Bucket> GetBuckets(string customerId, ProgressStage state, string userName)
        {
            return _repos.GetBuckets(customerId, state, userName);
        }

        /// <summary>
        /// SKU list of passed bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="stateFilter"> </param>
        /// <param name="activityFilter"> </param>
        /// <returns></returns>
        public IEnumerable<BucketSku> GetBucketSkuList(int bucketId)
        {
            return _repos.GetBucketSkuList(bucketId);
        }

        /// <summary>
        /// Pickslip of passed bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        public IEnumerable<Pickslip> GetBucketPickslip(int bucketId)
        {
            return _repos.GetBucketPickslips(bucketId);
        }

        /// <summary>
        /// Boxes of passed bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        public IEnumerable<Box> GetBucketBoxes(int bucketId)
        {
            return _repos.GetBucketBoxes(bucketId);
        }

        /// <summary>
        /// Edit bucket property. Error if you attempt to edit an unfrozen wave
        /// </summary>
        /// <param name="bucket"></param>
        internal Bucket UpdateWave(Bucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }

            var updatedWave = _repos.UpdateWave(bucket);
            return updatedWave;

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
        [Obsolete]
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

        public void FreezePickWave(int bucketId)
        {
            using (var trans = _repos.BeginTransaction())
            {
                _repos.DeleteBoxes(bucketId);
                _repos.SetFreezeStatus(bucketId, true);
                trans.Commit();
            }
        }

        public void UnfreezePickWave(int bucketId)
        {
            using (var trans = _repos.BeginTransaction())
            {
                _repos.CreateBoxes(bucketId);
                _repos.SetFreezeStatus(bucketId, false);
                trans.Commit();
            }
        }

        /// <summary>
        /// Increase priority by 1.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        public int IncrementPriority(int bucketId)
        {
            //return _repos.EditWave(new Bucket
            //{
            //    BucketId = bucketId,
            //    PriorityId = 1
            //}, EditBucketFlags.PriorityDelta);
            return _repos.UpdatePriority(bucketId, 1);
        }

        /// <summary>
        /// Decrease priority by 1.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        public int DecrementPriority(int bucketId)
        {
            //return _repos.EditWave(new Bucket
            //{
            //    BucketId = bucketId,
            //    PriorityId = -1
            //}, EditBucketFlags.PriorityDelta);
            return _repos.UpdatePriority(bucketId, -1);
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
