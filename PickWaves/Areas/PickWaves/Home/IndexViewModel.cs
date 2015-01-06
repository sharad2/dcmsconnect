using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Home
{
    /// <summary>
    /// Represents an entry used to display a link to customer filter
    /// </summary>
    public class CustomerFilterModel
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        /// <summary>
        /// Number of ordered pieces in all buckets combined
        /// </summary>
        [DisplayFormat(DataFormatString="{0:N0}")]
        public int OrderedPieces { get; set; }
    }

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

        private IList<CustomerFilterModel> _customerIdList;
        public IList<CustomerFilterModel> CustomerIdList
        {
            get
            {
                if (_customerIdList == null)
                {

                    var bucketCustomers = from item in BucketsByStatus
                                          from cust in item.Value
                                          select new CustomerFilterModel
                                          {
                                              CustomerId = cust.CustomerId,
                                              CustomerName = cust.CustomerName,
                                              OrderedPieces = cust.OrderedPieces
                                          };
                    var importedCustomers = from cust in ImportedOrders
                                            select new CustomerFilterModel
                                            {
                                                CustomerId = cust.CustomerId,
                                                CustomerName = cust.CustomerName,
                                                OrderedPieces = cust.PiecesOrdered
                                            };

                    var query = from item in bucketCustomers.Concat(importedCustomers)
                                group item by item.CustomerId into g
                                let pieces = g.Sum(p => p.OrderedPieces)
                                let name = g.Max(p => p.CustomerName)
                                orderby pieces descending, name
                                select new CustomerFilterModel
                                {
                                    CustomerId = g.Key,
                                    CustomerName = name,
                                    OrderedPieces = pieces
                                };
                    //_customerIdList = bucketCustomers.Concat(importedCustomers).Distinct(new CustomerFilterModelComparer()).OrderBy(p => p.CustomerName).ToList();
                    _customerIdList = query.ToList();

                }
                return _customerIdList;
            }
        }

        /// <summary>
        /// Total number of active buckets
        /// </summary>
        [DisplayFormat(DataFormatString="{0:N0}")]
        public int CountActivePickWaves
        {
            get
            {
                return BucketsByStatus.Sum(p => p.Value.Sum(q => q.BucketCount));
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
