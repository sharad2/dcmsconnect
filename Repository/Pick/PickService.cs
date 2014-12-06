using System;
using System.Collections.Generic;
using System.Linq;

namespace DcmsMobile.DcmsLite.Repository.Pick
{
    public class PickService : DcmsLiteServiceBase<PickRepository>
    {

        public string PrintBucket(int bucketId, int batchSize, string printerId)
        {
            return _repos.PrintBucket(bucketId, batchSize, printerId);
        }

        public IEnumerable<Bucket> GetBucketList(string buildingId, string customerId)
        {
            return _repos.GetBucketSummary(buildingId, null, customerId, 2000);
        }

        public Bucket GetBucket(int bucketId)
        {
            return _repos.GetBucketSummary(null, bucketId, null, null).FirstOrDefault();
        }


        public IEnumerable<Printer> GetPrinterList(string buildingId)
        {
            return _repos.GetPrinterList(buildingId);
        }

        internal IList<Box> GetBoxesOfBatch(string buildingId, string batchId)
        {
            return _repos.GetBoxesOfBatch(batchId);
        }

        internal PrintBatch GetBatch(string batchId)
        {
            if (string.IsNullOrWhiteSpace(batchId))
            {
                throw new ArgumentNullException("batchId");
            }
            return _repos.GetBatches(null, batchId, null).FirstOrDefault();
        }

        internal IList<PrintBatch> GetBatches(int bucketId)
        {
            return _repos.GetBatches(bucketId, null, null);
        }

        internal string GetBatchOfBox(string uccId)
        {
            var batch = _repos.GetBatches(null, null, uccId);
            if (batch != null && batch.Count > 0)
            {
                return batch.Select(p => p.BatchId).First();
            }
            return string.Empty;
        }

        internal void ReprintBatch(string batchId, string printer)
        {
            _repos.ReprintBatch(batchId, printer);
        }

        internal void PrintBoxLabel(string uccId, string printer)
        {
            _repos.PrintBoxLabel(uccId, printer);
        }
    }
}