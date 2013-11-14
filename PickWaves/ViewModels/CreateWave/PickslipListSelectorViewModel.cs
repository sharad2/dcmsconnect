using System.Web.Mvc;
using System.Web.Routing;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.PickWaves.ViewModels.CreateWave
{
    public class PickslipListSelectorViewModel : PickslipMatrixPartialViewModel
    {
        public PickslipListSelectorViewModel()
        {
        }

        public PickslipListSelectorViewModel(string customerId, int bucketId)
        {
            CustomerId = customerId;
            BucketId = bucketId;
        }

        public int BucketId { get; set; }

        public BucketModel Bucket { get; set; }
    }

    internal class PickslipListSelectorViewModelUnbinder : IModelUnbinder<PickslipListSelectorViewModel>
    {
        public void UnbindModel(RouteValueDictionary routeValueDictionary, string routeName, PickslipListSelectorViewModel model)
        {
            if (!string.IsNullOrEmpty(model.CustomerId))
            {
                routeValueDictionary.Add(model.NameFor(m => m.CustomerId), model.CustomerId);
            }
            if (model.BucketId != 0)
            {
                routeValueDictionary.Add(model.NameFor(m => m.BucketId), model.BucketId);
            }            
        }
    }
}