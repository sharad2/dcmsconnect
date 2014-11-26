using System.Collections.Generic;
using System.Web.Routing;
using EclipseLibrary.Mvc.Helpers;
using DcmsMobile.PickWaves.ViewModels;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    public class PickslipListViewModel : PickslipMatrixPartialViewModel
    {
        public PickslipListViewModel()
        {
        }

        public IList<PickslipModel> PickslipList { get; set; }

        public IList<int> SelectedPickslip { get; set; }

        public string CustomerName { get; set; }

        /// <summary>
        /// If user wants to add pickslip to specific bucket
        /// </summary>
        public int? BucketId { get; set; }

        public BucketModel Bucket { get; set; }
    }

    internal class PickslipListViewModelUnbinder : PickslipMatrixPartialViewModelUnbinder
    {
        public override void UnbindModel(RouteValueDictionary routeValueDictionary, string routeName, object value)
        {
            base.UnbindModel(routeValueDictionary, routeName, value);
            var model = value as PickslipListViewModel;

            if (model.BucketId > 0)
            {
                routeValueDictionary.Add(model.NameFor(m => m.BucketId), model.BucketId);
            }
        }

    }
}