using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.PickWaves.Repository.BoxPickPallet
{
    internal class BoxPickPalletService : PickWaveServiceBase<BoxPickPalletRepository>
    {

        #region Intialization

        public BoxPickPalletService(TraceContext ctx, string userName, string clientInfo)
        {
            _repos = new BoxPickPalletRepository(ctx, userName, clientInfo);
        }

        #endregion

        public BoxPickBucket GetBucketDetail(int bucketId)
        {
            return _repos.GetBucketDetail(bucketId);
        }

        public IEnumerable<Pallet> GetPalletsOfBucket(int bucketId)
        {
            return _repos.GetPallets(bucketId, null);
        }

        public Pallet GetPallet(string palletId)
        {
            return _repos.GetPallets(null, palletId).FirstOrDefault();
        }

        public int CreatePallet(int bucketId, string palletId, int palletLimit)
        {
            return _repos.CreatePallet(bucketId, palletId, palletLimit);
        }

        public void RemoveUnPickedBoxesFromPallet(string palletId)
        {
            _repos.RemoveUnPickedBoxesFromPallet(palletId);
        }

        public int? GetBucketToExpedite()
        {
            return _repos.GetBucketToExpedite();
        }
    }
}