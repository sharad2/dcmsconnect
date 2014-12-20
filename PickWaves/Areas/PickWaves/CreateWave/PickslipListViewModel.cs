using DcmsMobile.PickWaves.ViewModels;
using System;
using System.Collections.Generic;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    public class PickslipListViewModel :ViewModelBase
    {
        public PickslipListViewModel()
        {
        }

        public string CustomerId { get; set; }

        public PickslipDimension RowDimIndex { get; set; }

        public PickslipDimension ColDimIndex { get; set; }

        public string ColDimVal { get; set; }

        public string RowDimVal { get; set; }

        public string VwhId { get; set; }

        public string RowDimDisplayName { get; set; }

        public string ColDimDisplayName { get; set; }
   

        public IList<PickslipModel> PickslipList { get; set; }

        public string CustomerName { get; set; }

        /// <summary>
        /// If user wants to add pickslip to specific bucket
        /// </summary>
        public int? BucketId { get; set; }

        public BucketModel Bucket { get; set; }

    }

    //internal class PickslipListViewModelUnbinder : PickslipMatrixPartialViewModelUnbinder
    //{
    //    public override void UnbindModel(RouteValueDictionary routeValueDictionary, string routeName, object value)
    //    {
    //        base.UnbindModel(routeValueDictionary, routeName, value);
    //        var model = value as PickslipListViewModel;

    //        if (model.BucketId > 0)
    //        {
    //            routeValueDictionary.Add(model.NameFor(m => m.BucketId), model.BucketId);
    //        }
    //    }

    //}
}