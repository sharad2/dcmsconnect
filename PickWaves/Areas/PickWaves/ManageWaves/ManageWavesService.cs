using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository;
using System;
using System.Collections.Generic;
using System.Data.Common;
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

        ///// <summary>
        ///// All parameters are optional. If none are specified, all buckets will be retrieved 
        ///// which is definitely what you will not want.
        ///// </summary>
        ///// <param name="customerId"></param>
        ///// <param name="state"></param>
        ///// <returns></returns>
        //internal IList<BucketWithActivities> GetBuckets(string customerId, ProgressStage state, string userName)
        //{
        //    return _repos.GetBuckets(null, customerId, state, userName);
        //}

        /// <summary>
        /// SKU list of passed bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="stateFilter"> </param>
        /// <param name="activityFilter"> </param>
        /// <returns></returns>
        public IList<BucketSku> GetBucketSkuList(int bucketId)
        {
            return _repos.GetBucketSkuList(bucketId);
        }

        /// <summary>
        /// Pickslip of passed bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        public IList<Pickslip> GetBucketPickslip(int bucketId)
        {
            return _repos.GetBucketPickslips(bucketId);
        }

        /// <summary>
        /// Boxes of passed bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        public IList<Box> GetBucketBoxes(int bucketId)
        {
            return _repos.GetBucketBoxes(bucketId);
        }

        /// <summary>
        /// Edit bucket property. Error if you attempt to edit an unfrozen wave. This should be called within a transaction so that the update can be cancelled
        /// if the wave is not frozen
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="trans">Required only to ensure that the caller has created a transaction</param>
        internal BucketEditable UpdateWave(int bucketId, BucketEditable bucket, DbTransaction trans)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }

            var updatedWave = _repos.UpdateWave(bucketId, bucket);
            if (!bucket.IsFrozen)
            {
                throw new InvalidOperationException("Only frozen pick waves can be edited");
            }
            return updatedWave;

        }

        public void RemovePickslipFromBucket(long[] pickslipId, int bucketId)
        {
            _repos.RemovePickslipFromBucket(pickslipId, bucketId);
        }

        public void FreezePickWave(int bucketId, DbTransaction trans)
        {
            _repos.DeleteBoxes(bucketId);
            _repos.SetFreezeStatus(bucketId, true);
            trans.Commit();
        }

        public void UnfreezePickWave(int bucketId, DbTransaction trans)
        {
            _repos.CreateBoxes(bucketId);
            _repos.SetFreezeStatus(bucketId, false);
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
        public IList<BucketArea> GetBucketAreas(int bucketId)
        {
            return _repos.GetBucketAreas(bucketId);
        }

        public BucketEditable GetEditableBucket(int bucketId)
        {
            return _repos.GetEditableBucket(bucketId);
        }

        internal void CancelBoxes(string[] boxes)
        {
            _repos.CancelBoxes(boxes);
        }

        public IList<CustomerBucket> GetBucketList(string customerId, string userName)
        {
            return _repos.GetBucketList(customerId, userName);
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
