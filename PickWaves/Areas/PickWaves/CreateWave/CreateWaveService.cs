using DcmsMobile.PickWaves.Repository;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Web;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    internal class CreateWaveService : PickWaveServiceBase<CreateWaveRepository>
    {
        #region Intialization

        public CreateWaveService(TraceContext ctx, string userName, string clientInfo)
        {
            _repos = new CreateWaveRepository(ctx, userName, clientInfo);
        }

        #endregion

        public IEnumerable<Pickslip> GetPickslipList(string customerId, string vwhId, PickslipDimension dimRow, string dimRowVal, PickslipDimension dimCol, string dimColVal)
        {
            return _repos.GetPickslips(customerId, vwhId, new[] { Tuple.Create(dimRow, (object)dimRowVal), Tuple.Create(dimCol, (object)dimColVal) });
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
        internal CustomerOrderSummary GetOrderSummary(string customerId, string vwhId, PickslipDimension dimRow, PickslipDimension dimCol)
        {
            return _repos.GetOrderSummaryForCustomer(customerId, vwhId, dimRow, dimCol);
        }


        public DbTransaction BeginTransaction()
        {

            return _repos.BeginTransaction();

        }

        public int CreateDefaultWave()
        {
            return _repos.CreateDefaultWave();
        }

        public void AddPickslipsPerDim(int bucketId, string customerId,
            PickslipDimension dimRow, DimensionValue dimRowVal,
            PickslipDimension dimCol, DimensionValue dimColVal, string vwhId)
        {
            _repos.AddPickslipsPerDim(bucketId, customerId, new[] { 
                Tuple.Create(dimRow, dimRowVal),
                Tuple.Create(dimCol, dimColVal) 
            }, vwhId, false);
        }

        internal void AddPickslipsToWave(int bucketId, IList<long> pickslipList)
        {
            _repos.AddPickslipsToWave(bucketId, pickslipList);
        }


        /// <summary>
        /// Returns the list of VWh ID of passed customer orders
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public IList<VirtualWarehouse> GetVWhListOfCustomer(string customerId)
        {
            return _repos.GetVWhListOfCustomer(customerId);
        }

        public PickWave GetPickWave(int bucketId)
        {
            return _repos.GetPickWave(bucketId);
        }
    }
}