using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository.Home;
using DcmsMobile.PickWaves.ViewModels;
using DcmsMobile.PickWaves.ViewModels.Home;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Controllers
{
    public partial class HomeController : PickWavesControllerBase
    {

        #region Intialization

        public HomeController()
        {

        }

        private HomeService _service;

        protected override void Initialize(RequestContext ctx)
        {
            base.Initialize(ctx);
            var clientInfo = string.IsNullOrEmpty(ctx.HttpContext.Request.UserHostName) ?
                             ctx.HttpContext.Request.UserHostAddress : ctx.HttpContext.Request.UserHostName;
            if (_service == null)
            {
                _service = new HomeService(HttpContext.Trace,
                    HttpContext.User.IsInRole(ROLE_WAVE_MANAGER) ? HttpContext.User.Identity.Name : string.Empty, clientInfo);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_service != null)
            {
                _service.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        /// Showing list of bucket summary for all customer.
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Index()
        {
            return ShowHomePage(SearchTextType.Unknown, null);
        }


        public virtual ActionResult Customer(string customerId)
        {
            return ShowHomePage(SearchTextType.CustomerId, customerId);
        }


        public virtual ActionResult Search(string searchText)
        {
            var search = SearchTextType.Unknown;
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.Trim().ToUpper();
                search = _service.ParseSearchText(searchText);
            }

            switch (search)
            {
                // TC1: When search text is unknown.
                case SearchTextType.Unknown:
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        AddStatusMessage(string.Format("Search text {0} was not understood and is being ignored", searchText));
                    }
                    if (Request.UrlReferrer != null)
                    {
                        return Redirect(Request.UrlReferrer.PathAndQuery);
                    }
                    break;

                // TC2: When search text is bucket id.
                case SearchTextType.BucketId:
                    return RedirectToAction(MVC_PickWaves.PickWaves.ManageWaves.Wave(new DcmsMobile.PickWaves.ViewModels.ManageWaves.WaveViewModel(int.Parse(searchText),
                         DcmsMobile.PickWaves.ViewModels.ManageWaves.SuggestedNextActionType.SearchAgain)));

                // TC3: When search text is customer id.
                case SearchTextType.CustomerId:
                    AddStatusMessage(string.Format("Filter applied for Customer: {0}", searchText));
                    break;               

                default:
#if DEBUG
                    throw new NotImplementedException();
#else
                    break;
#endif
            }
            return ShowHomePage(search, searchText);
        }

        private ActionResult ShowHomePage(SearchTextType search, string searchText)
        {
            var bucketSummary = _service.GetBucketSummary(search, searchText);

            var query = from bucket in Map(bucketSummary)
                        group bucket by bucket.BucketStatus into g
                        orderby g.Key
                        select new
                        {
                            Status = g.Key,
                            // Inactive customers are displayed last
                            Buckets = (from item in g
                                       orderby item.DcCancelDateRange.From,
                                       item.IsCustomerActive ? 0 : 1
                                       select item).ToArray()
                        };

            var model = new IndexViewModel
            {
                IsFilterApplied = search != SearchTextType.Unknown
            };
            foreach (var item in query)
            {
                model.BucketsByStatus.Add(item.Status, item.Buckets);
            }
            model.ImportedOrders = (from item in _service.GetImportedOrderSummary(search, searchText)
                                    orderby item.MinDcCancelDate, item.DollarsOrdered descending
                                    select new ImportedOrderSummaryModel
                                    {
                                        CustomerId = item.Customer.CustomerId,
                                        CustomerName = item.Customer.Name,
                                        IsCustomerActive = item.Customer.IsActive,
                                        PickslipCount = item.PickslipCount,
                                        PiecesOrdered = item.PiecesOrdered,
                                        DollarsOrdered = (double)item.DollarsOrdered,
                                        DcCancelDateRange = new DateRange
                                        {
                                            From = item.MinDcCancelDate,
                                            To = item.MaxDcCancelDate
                                        },
                                        ImportDateRange = new DateRange
                                        {
                                            From = item.MinPickslipImportDate,
                                            To = item.MaxPickslipImportDate
                                        }
                                    }).ToArray();
            return View(Views.Index, model);
        }



        /// <summary>
        /// </summary>
        ///<param name="bucketSummary">
        ///</param>
        /// <returns>
        /// Return bucket summery of passed customer
        /// </returns>
        private IEnumerable<CustomerBucketStateModel> Map(IEnumerable<BucketSummary> bucketSummary)
        {
            return from item in bucketSummary
                   orderby item.MinDcCancelDate, item.OrderedPieces descending
                   group item by new
                   {
                       BucketStatus = item.BucketState,
                       item.Customer.CustomerId
                   } into g
                   let maxPitchArea = g.Select(p => p.MaxPitchArea).First()
                   let minPitchArea = g.Select(p => p.MinPitchArea).First()
                   let maxPullArea = g.Select(p => p.MaxPullArea).First()
                   let minPullArea = g.Select(p => p.MinPullArea).First()
                   select new CustomerBucketStateModel
                   {
                       BucketStatus = g.Key.BucketStatus,
                       BucketCount = g.Sum(p => p.BucketCount),
                       CustomerId = g.Key.CustomerId,
                       CustomerName = g.First().Customer.Name,
                       IsCustomerActive = g.First().Customer.IsActive,
                       PickedPieces = g.Sum(p => p.CurrentPieces),
                       OrderedPieces = g.Sum(p => p.OrderedPieces),
                       ExpectedPieces = g.Sum(p => p.ExpectedPieces),
                       //CancelledPieces = g.Sum(p => p.CancelledPieces),
                       PitchAreaCount = g.Sum(p => p.PitchAreaCount),
                       PullAreaCount = g.Sum(p => p.PullAreaCount),
                       MaxPriorityId = g.Sum(p => p.MaxPriorityId),
                       PickingDateRange = new DateRange
                       {
                           To = g.Select(p => p.MaxPitchingEndDate).FirstOrDefault(),
                           From = g.Select(p => p.MinPitchingEndDate).FirstOrDefault(),
                           ShowTime = true
                       },
                       DcCancelDateRange = new DateRange
                       {
                           To = g.Select(p => p.MaxDcCancelDate).FirstOrDefault(),
                           From = g.Select(p => p.MinDcCancelDate).FirstOrDefault(),
                       },
                       MaxPitchArea = new InventoryAreaModel(maxPitchArea),
                       MinPitchArea = new InventoryAreaModel(minPitchArea),
                       MaxPullArea = new InventoryAreaModel(maxPullArea),
                       MinPullArea = new InventoryAreaModel(minPullArea)
                   };
        }

        protected override string ManagerRoleName
        {
            get { return ROLE_WAVE_MANAGER; }
        }
    }
}