using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

        /// <summary>
        /// The layout page displays a Show All link when this is true. Individual actions are expected to set this..
        /// </summary>
        public bool IsUserNameFilterApplied { get; set; }

        /// <summary>
        /// The layout page displays a Show All link when this is true. Individual actions are expected to set this..
        /// </summary>
        public bool IsCustomerFilterApplied { get; set; }

        public bool IsAnyFilterApplied
        {
            get
            {
                return IsUserNameFilterApplied || IsCustomerFilterApplied;
            }
        }

        private IList<Tuple<string, string>> _customerIdList;
        public IList<Tuple<string, string>> CustomerIdList
        {
            get
            {
                if (_customerIdList == null)
                {
                    var bucketCustomers = BucketsByStatus.SelectMany(p => p.Value).Select(p => Tuple.Create(p.CustomerId,p.CustomerName));
                    var importedCustomers = ImportedOrders.Select(p => Tuple.Create(p.CustomerId, p.CustomerName));
                    _customerIdList = bucketCustomers.Concat(importedCustomers).Distinct().OrderBy(p => p.Item2).ToList();

                }
                return _customerIdList;
            }
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
