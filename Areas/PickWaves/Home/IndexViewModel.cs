using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.ViewModels;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Home
{
    /// <summary>
    /// The model passed to the Index view
    /// </summary>
    public class IndexViewModel : ViewModelBase
    {
        private readonly SortedList<ProgressStage, IList<CustomerBucketStateModel>> _bucketsByStatus;
        public IndexViewModel()
        {
            _bucketsByStatus = new SortedList<ProgressStage, IList<CustomerBucketStateModel>>(4);
        }

        /// <summary>
        /// List of imported orders for which buckets have not been created
        /// </summary>
        public IList<ImportedOrderSummaryModel> ImportedOrders { get; set; }

        public SortedList<ProgressStage, IList<CustomerBucketStateModel>> BucketsByStatus
        {
            get
            {
                return _bucketsByStatus;
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalImportedPickslipCount
        {
            get
            {
                return ImportedOrders.Sum(p => p.PickslipCount);
            }
        }

        [DisplayFormat(DataFormatString = "${0:N0}")]
        public double TotalImportedDollarsOrdered
        {
            get
            {
                return ImportedOrders.Sum(p => p.DollarsOrdered);
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalImportedPiecesOrdered
        {
            get
            {
                return ImportedOrders.Sum(p => p.PiecesOrdered);
            }
        }

        private Lazy<IDictionary<ProgressStage, string>> _lazyStateDisplayNames =
            new Lazy<IDictionary<ProgressStage, string>>(() => PickWaveHelpers.GetEnumMemberAttributes<ProgressStage, DisplayAttribute>().ToDictionary(p => p.Key, p => p.Value.Name));

        public IDictionary<ProgressStage, string> StateDisplayNames
        {
            get
            {
                return _lazyStateDisplayNames.Value;
            }
        }

        [DataType(DataType.Text)]
        public DateRange ImportDateRange
        {
            get
            {
                return new DateRange
                {
                    From = ImportedOrders.Min(p => p.ImportDateRange.From),
                    To = ImportedOrders.Max(p => p.ImportDateRange.To)
                };
            }
        }

        [DataType(DataType.Text)]
        public DateRange DcCancelDateRange
        {
            get
            {
                return new DateRange
                {
                    From = ImportedOrders.Min(p => p.DcCancelDateRange.From),
                    To = ImportedOrders.Max(p => p.DcCancelDateRange.To)
                };
            }
        }

        public string SearchUserName { get; set; }

        public string SearchCustomerId { get; set; }
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
