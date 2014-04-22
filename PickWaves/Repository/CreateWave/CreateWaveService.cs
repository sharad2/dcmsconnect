﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.PickWaves.Repository.CreateWave
{
    public class CreateWaveService : PickWaveServiceBase<CreateWaveRepository>
    {
        #region Intialization

        public CreateWaveService(TraceContext ctx, string userName, string clientInfo)
        {
            _repos = new CreateWaveRepository(ctx, userName, clientInfo);
        }

        #endregion

        public IEnumerable<Pickslip> GetPickslipList(string customerId, PickslipDimension dimRow, string dimRowVal, PickslipDimension dimCol, string dimColVal)
        {
            return _repos.GetPickslips(customerId, new[] { Tuple.Create(dimRow, (object)dimRowVal), Tuple.Create(dimCol, (object)dimColVal) });
        }

        /// <summary>
        /// Special handling priority to provide numeric sort
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vwhId"> </param>
        /// <param name="dimRow"> </param>
        /// <param name="dimCol"> </param>
        /// <returns></returns>
        /// <remarks>
        /// If too many rows for the dimension are returned, then null is returned. If no rows are returned for the dimension, and empty collection is returned.
        /// Thus null is different from empty.
        /// </remarks>
        internal Tuple<IList<CustomerOrderSummary>, IDictionary<PickslipDimension, int>> GetOrderSummary(string customerId, string vwhId, PickslipDimension dimRow, PickslipDimension dimCol)
        {
            var result = _repos.GetOrderSummaryForCustomer(customerId, vwhId, dimRow, dimCol);
            // If dim is priority, pad the dim value
            //if (dimRow == PickslipDimension.Priority)
            //{
            //    // For priority we want numeric sort even though priority is stored in a string column
            //    return result.OrderBy(p => ((string)p.DimensionValue).PadLeft(8));
            //}
            return result;
        }

        public int CreateWave(PickWaveEditable bucket, string customerId, PickslipDimension dimRow, string dimRowVal, PickslipDimension dimCol, string dimColVal, string vwhId)
        {
            using (var trans = _repos.BeginTransaction())
            {
                var bucketId = _repos.CreateWave(bucket);
                _repos.AddPickslipsPerDim(bucketId, customerId, new[] { Tuple.Create(dimRow, (object)dimRowVal), Tuple.Create(dimCol, (object)dimColVal) }, vwhId, true);
                trans.Commit();
                return bucketId;
            }
        }

        public void AddPickslipsPerDim(int bucketId, string customerId, PickslipDimension dimRow, string dimRowVal, PickslipDimension dimCol, string dimColVal, string vwhId)
        {
            _repos.AddPickslipsPerDim(bucketId, customerId, new[] { Tuple.Create(dimRow, (object)dimRowVal), Tuple.Create(dimCol, (object)dimColVal) }, vwhId, false);
        }

        internal void AddPickslipsToWave(int bucketId, IList<int> pickslipList)
        {
            _repos.AddPickslipsToWave(bucketId, pickslipList);
        }

        public IList<CreateWaveArea> GetAreasForCustomer(string customerId)
        {
            return _repos.GetAreasForCustomer(customerId);
        }

        /// <summary>
        /// Returns the list of VWh ID of passed customer orders
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public IEnumerable<VirtualWarehouse> GetVWhListOfCustomer(string customerId)
        {
            return _repos.GetVWhListOfCustomer(customerId);
        }

        public PickWaveEditable GetEditableBucket(int bucketId)
        {
            return _repos.GetEditableBucket(bucketId);
        }
    }
}