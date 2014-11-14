using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Create wave and then add pickslips to this.
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="customerId"></param>
        /// <param name="dimRow"></param>
        /// <param name="dimRowVal"></param>
        /// <param name="dimCol"></param>
        /// <param name="dimColVal"></param>
        /// <param name="vwhId"></param>
        /// <returns></returns>
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