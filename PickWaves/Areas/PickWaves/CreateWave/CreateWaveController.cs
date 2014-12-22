using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.ViewModels;
using EclipseLibrary.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;


namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    [AuthorizeEx("Managing Pick Waves requires role {0}", Roles = ROLE_WAVE_MANAGER)]
    [RoutePrefix("create")]
    public partial class CreateWaveController : PickWavesControllerBase
    {
        #region Intialization
        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public CreateWaveController()
        {

        }

        private Lazy<CreateWaveService> _service;

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (_service == null)
            {
                _service = new Lazy<CreateWaveService>(() => new CreateWaveService(this.HttpContext.Trace,
                    HttpContext.User.IsInRole(ROLE_WAVE_MANAGER) ? HttpContext.User.Identity.Name : string.Empty,
                    Request.UserHostName ?? Request.UserHostAddress)
                    );
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_service != null && _service.IsValueCreated)
            {
                _service.Value.Dispose();
            }
            _service = null;
            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        /// Applies {0:d} to date time values. Otherwise leaves the value alone
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string FormatDimensionValue(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            if (value is DateTime)
            {
                return string.Format("{0:d}", value);
            }
            return value.ToString();
        }

        private const string COOKIE_PICKWAVE = "PICKWAVE_ALL_COOKIES";

        private const string COOKIE_ROW_DIMENSION = "SELECTED_ROW_DIMENSION";
        private const string COOKIE_COL_DIMENSION = "SELECTED_COL_DIMENSION";

        //private static string FormatDimensionValue(object value)
        //{
        //    if (value == null)
        //    {
        //        return string.Empty;
        //    }
        //    if (value is DateTime)
        //    {
        //        return string.Format("{0:d}", value);
        //    }
        //    return value.ToString();
        //}

        /// <summary>
        /// Populates the passed PickslipMatrixPartialViewModel
        /// </summary>
        /// <param name="model"> </param>
        /// <param name="customerId"> </param>
        /// <param name="selectedRowDimIndex"> </param>
        /// <param name="selectedColDimIndex"> </param>
        /// <returns>Whether pickslips exist which can be added to a bucket</returns>
        /// <remarks>
        /// Passed selectedRowDimIndex and selectedColDimIndex are ignored if it turns out that either of them have no pickslips
        /// </remarks>
        private bool PopulatePickslipMatrixPartialModel(IndexViewModel model)
        {
            model.VwhList = (from item in _service.Value.GetVWhListOfCustomer(model.CustomerId)
                             select new SelectListItem
                             {
                                 Text = item.VWhId + " : " + item.Description,
                                 Value = item.VWhId,
                                 Selected = item.VWhId == model.VwhId
                             }).ToList();

            if (model.VwhList.Count == 0)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(model.VwhId))
            {
                // If Vwh has not been passed, use this one
                model.VwhId = model.VwhList.Select(p => p.Value).First();
            }

            var summary = _service.Value.GetOrderSummary(model.CustomerId, model.VwhId, model.RowDimIndex, model.ColDimIndex);

            if (summary.CountValuesPerDimension == null)
            {
                // No more pickslips which can be added to a bucket. The dimension matrix will be empty.
                // This happens when the last set of pickslips has been added to the bucket.
                return false;
            }
            var requery = false;
            if (summary.CountValuesPerDimension[model.RowDimIndex] == 0)
            {
                model.RowDimIndex = summary.CountValuesPerDimension.First(p => p.Value > 0).Key;
                requery = true;
            }

            if (summary.CountValuesPerDimension[model.ColDimIndex] == 0)
            {
                model.ColDimIndex = summary.CountValuesPerDimension.First(p => p.Value > 0).Key;
                requery = true;
            }

            if (requery)
            {
                summary = _service.Value.GetOrderSummary(model.CustomerId, model.VwhId, model.RowDimIndex, model.ColDimIndex);
            }

            model.RowDimDisplayName = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()[model.RowDimIndex].Name;
            model.ColDimDisplayName = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()[model.ColDimIndex].Name;



            const int MAX_COL_DIMENSIONS = 30;

            model.RowDimensionList = (from kvp in PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()
                                      let count = summary.CountValuesPerDimension[kvp.Key]
                                      select new SelectListItem
                                      {
                                          Value = count == 0 ? "" : ((int)(kvp.Key)).ToString(),
                                          Text = string.Format("{0} ({1:N0})", kvp.Value.Name, count),
                                          Selected = kvp.Key == model.RowDimIndex
                                      }).OrderBy(p => p.Text).ToArray();

            // Dimensions which have too many distinct values are not displayed as column dimensions
            model.ColDimensionList = (from kvp in PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()
                                      let count = summary.CountValuesPerDimension[kvp.Key]
                                      where count <= MAX_COL_DIMENSIONS
                                      select new SelectListItem
                                      {
                                          Value = count > MAX_COL_DIMENSIONS || count == 0 ? "" : ((int)(kvp.Key)).ToString(),
                                          Text = string.Format("{0} ({1:N0})", kvp.Value.Name, count),
                                          Selected = kvp.Key == model.ColDimIndex
                                      }).OrderBy(p => p.Text).ToArray();

            //model.Rows = (from rowVal in summary.AllValues.RowValues
            //              let row = summary.AllValues.GetRow(rowVal)
            //              select new RowDimensionModel
            //              {
            //                  PickslipCounts = row.ToDictionary(p => FormatDimensionValue(p.Key), p => p.Value.PickslipCount),
            //                  OrderedPieces = row.ToDictionary(p => FormatDimensionValue(p.Key), p => p.Value.OrderedPieces),
            //                  DimensionValue = FormatDimensionValue(rowVal)
            //              }).ToArray();

            model.Rows = (from rowVal in summary.AllValues3.FirstKeys
                          //let row = summary.AllValues.GetRow(rowVal)
                          select new RowDimensionModel
                          {
                              PickslipCounts = summary.AllValues3.FirstKeyValues(rowVal).ToDictionary(p => FormatDimensionValue(p.Item1), p => p.Item2.PickslipCount),
                              OrderedPieces = summary.AllValues3.FirstKeyValues(rowVal).ToDictionary(p => FormatDimensionValue(p.Item1), p => p.Item2.OrderedPieces),
                              DimensionValue = FormatDimensionValue(rowVal)
                          }).ToArray();

            //var list = new List<RowDimensionModel>();

            //foreach (var rowVal in summary.AllValues2.RowValues())
            //{
            //    var x = new RowDimensionModel
            //                {
            //                    PickslipCounts = summary.AllValues2.PickslipCounts(rowVal),
            //                    OrderedPieces = summary.AllValues2.OrderedPieces(rowVal),
            //                    DimensionValue = SparseMatrix.FormatValue(rowVal)
            //                };
            //    list.Add(x);
            //}

            //model.ColDimensionValues = summary.AllValues.ColValues.Select(p => FormatDimensionValue(p)).ToArray();


            return true;
        }



        /// <summary>
        /// Displays the add pickslips page using the supplied defaults. All parameters are optional except customerId
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="rowDimIndex"></param>
        /// <param name="colDimIndex"></param>
        /// <param name="vwhId"></param>
        /// <param name="pullAreaId"></param>
        /// <param name="pitchAreaId"></param>
        /// <param name="lastBucketId"></param>
        /// <returns></returns>
        [Route]
        public virtual ActionResult Index(string customerId,
            PickslipDimension rowDimIndex = PickslipDimension.NotSet,
            PickslipDimension colDimIndex = PickslipDimension.NotSet,
            string vwhId = null,
            int? lastBucketId = null)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new NotImplementedException();
            }

            var model = new IndexViewModel();
            //Showing only those area where order of customer are available.
            //var areas = _service.Value.GetAreasForCustomer(customerId);



            #region Manage Cookie
            //  SelectedDimension : If null, then a reasonable default is used. The default is cookie => factory default
            //  If non null, then it is used and written to a cookie.
            var cookie = Request.Cookies[COOKIE_PICKWAVE];

            if (rowDimIndex == PickslipDimension.NotSet || colDimIndex == PickslipDimension.NotSet)
            {
                // Read cookie
                int dimRowIndex, dimColIndex;
                if (cookie != null && cookie[COOKIE_ROW_DIMENSION] != null && int.TryParse(cookie[COOKIE_ROW_DIMENSION], out dimRowIndex) &&
                    int.TryParse(cookie[COOKIE_COL_DIMENSION], out dimColIndex))
                {
                    model.RowDimIndex = (PickslipDimension)dimRowIndex;
                    model.ColDimIndex = (PickslipDimension)dimColIndex;
                }
                else
                {
                    // Factory defaults
                    model.RowDimIndex = PickslipDimension.CustomerDcCancelDate;
                    model.ColDimIndex = PickslipDimension.CustomerDc;
                }
            }
            else
            {
                model.RowDimIndex = rowDimIndex;
                model.ColDimIndex = colDimIndex;
                // Write cookie
                if (cookie == null)
                {
                    cookie = new HttpCookie(COOKIE_PICKWAVE) { Expires = DateTime.Now.AddDays(7) };
                    cookie.Values.Add(COOKIE_ROW_DIMENSION, model.RowDimIndex.ToString());
                    cookie.Values.Add(COOKIE_COL_DIMENSION, model.ColDimIndex.ToString());
                }
                else
                {
                    if (cookie[COOKIE_ROW_DIMENSION] != null)
                        cookie.Values.Set(COOKIE_ROW_DIMENSION, model.RowDimIndex.ToString());
                    if (cookie[COOKIE_COL_DIMENSION] != null)
                        cookie.Values.Set(COOKIE_COL_DIMENSION, model.ColDimIndex.ToString());
                }
                HttpContext.Response.Cookies.Add(cookie);
            }

            #endregion

            model.CustomerId = customerId;

            var cust = _service.Value.GetCustomer(model.CustomerId);
            model.CustomerName = cust == null ? model.CustomerId : cust.Name;


            // Make sure that selected row and dimension are within the bounds of their respective drop downs
            if (!PopulatePickslipMatrixPartialModel(model))
            {
                var nopickslip = new IndexNoPickslipsViewModel
                {
                    BucketId = lastBucketId,
                    CustomerId = customerId,
                    CustomerName = model.CustomerName
                };
                return View(Views.IndexNoPickslips, nopickslip);
            }



            // If Bucket is created.
            if (lastBucketId.HasValue)
            {
                // Retrive some information of bucket.
                model.LastBucketId = lastBucketId;
                var bucket = _service.Value.GetPickWave(model.LastBucketId.Value);
                if (bucket != null)
                {
                    model.PullAreaShortName = bucket.PullAreaShortName;
                    model.PitchAreaShortName = bucket.PitchAreaShortName;
                    model.PickslipCount = bucket.PickslipCount;
                }
            }
            return View(Views.Index, model);
        }


        /// <summary>
        /// Add pickslips to passed bucket.Or if bucket is not created then create it first, then add pickslip.
        /// </summary>
        /// <param name="model">
        /// RowDimIndex,ColDimIndex,CustomerId,LastBucketId
        /// Optional : RequiredBoxExpediting,PullAreaId,PitchAreaId,QuickPitch
        /// </param>
        /// <param name="viewPickslips">
        /// Showing the pickslip list
        /// </param>
        /// <returns></returns>
        [HttpPost]
        [Route("adddim")]
        public virtual ActionResult AddPickslipsOfDim(int? bucketId, string customerId,
            PickslipDimension rowDimIndex, string rowDimVal,
             PickslipDimension colDimIndex, string colDimVal,
            string vwhId)
        {

            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentNullException("customerId");
            }
            if (string.IsNullOrWhiteSpace(vwhId))
            {
                throw new ArgumentNullException("vwhId");
            }

            using (var trans = _service.Value.BeginTransaction())
            {
                // If bucket is not created.create it first.
                if (bucketId == null)
                {
                    //Now Create bucket
                    bucketId = _service.Value.CreateDefaultWave();
                    AddStatusMessage(string.Format("Pick Wave {0} created.", bucketId));
                }
                // Add pickslip to bucket 
                _service.Value.AddPickslipsPerDim(bucketId.Value, customerId,
                    rowDimIndex,
                    rowDimVal, colDimIndex, colDimVal, vwhId);
                trans.Commit();
            }
            AddStatusMessage(string.Format("Pickslips added to Pick Wave {0}", bucketId));

            return RedirectToAction(Actions.Index(customerId, rowDimIndex, colDimIndex,
                vwhId, bucketId));
        }

        /// <summary>
        /// Get pickslip list of passed criteria.
        /// </summary>
        /// <param name="model">
        /// model.CustomerId, model.RowDimIndex, model.ColDimIndex, model.RowDimVal, model.ColDimVal,model.BucketId
        /// </param
        /// <returns></returns>
        [Route("ps")]
        public virtual ActionResult PickslipList(
            [Required]
            string customerId,
            PickslipDimension rowDimIndex,
            [Required]
            string rowDimVal,
            PickslipDimension colDimIndex,
            [Required]
            string colDimVal,
            [Required]
            string vwhId,
            int? bucketId)
        {

            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentNullException("customerId");
            }
            if (string.IsNullOrWhiteSpace(vwhId))
            {
                throw new ArgumentNullException("vwhId");
            }
            if (string.IsNullOrWhiteSpace(rowDimVal))
            {
                throw new ArgumentNullException("rowDimVal");
            }
            if (string.IsNullOrWhiteSpace(colDimVal))
            {
                throw new ArgumentNullException("colDimVal");
            }
            var model = new PickslipListViewModel();
            model.CustomerId = customerId;

            model.RowDimIndex = rowDimIndex;
            model.ColDimIndex = colDimIndex;

            model.VwhId = vwhId;
            model.RowDimVal = rowDimVal;
            model.ColDimVal = colDimVal;
            // Pickslip list of passed dimension.
            var pickslips = _service.Value.GetPickslipList(model.CustomerId, model.VwhId, model.RowDimIndex, model.RowDimVal, model.ColDimIndex, model.ColDimVal);
            model.PickslipList = (from pickslip in pickslips
                                  let routePickslip = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchPickslipImported1]
                                  // let routePickslip = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchPickslip1]
                                  let routePo = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchPoImported3]
                                  select new PickslipModel(pickslip)
                                  {
                                      UrlInquiryPickslip = routePickslip == null ? null : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchPickslipImported1, new
                                      {
                                          id = pickslip.PickslipId
                                      }),
                                      UrlInquiryPurchaseOrder = routePo == null ? null : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchPoImported3, new
                                            {
                                                id = pickslip.PurchaseOrder,
                                                pk1 = pickslip.CustomerId,
                                                pk2 = pickslip.Iteration
                                            })
                                  }).ToArray();
            model.ManagerRoleName = ROLE_WAVE_MANAGER;

            model.RowDimDisplayName = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()[model.RowDimIndex].Name;
            model.ColDimDisplayName = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()[model.ColDimIndex].Name;
            model.RowDimVal = string.IsNullOrEmpty(model.RowDimVal) ? RowDimensionModel.NULL_DIMENSION_VALUE : model.RowDimVal;
            model.CustomerName = (_service.Value.GetCustomer(model.CustomerId) == null ? "" : _service.Value.GetCustomer(model.CustomerId).Name);

            model.BucketId = bucketId;

            if (model.BucketId.HasValue)
            {
                var bucket = _service.Value.GetBucket(model.BucketId.Value);
                model.Bucket = new BucketModel(bucket);
            }
            return View(Views.PickslipList, model);
        }

        /// <summary>
        /// Add passed pickslips to passed bucket.
        /// </summary>
        /// <param name="model"> </param>
        /// <returns></returns>
        [HttpPost]
        [Route("addps")]
        public virtual ActionResult AddPickslipsToBucket(int? bucketId, long[] pickslips, string customerId, PickslipDimension rowDimIndex,
            string rowDimVal, PickslipDimension colDimIndex, string colDimVal, string vwhId)
        {
            if (pickslips == null || pickslips.Length == 0)
            {
                AddStatusMessage("Please select pickslip.");
            }
            else
            {
                using (var trans = _service.Value.BeginTransaction())
                {
                    if (bucketId == null)
                    {
                        bucketId = _service.Value.CreateDefaultWave();
                        AddStatusMessage(string.Format("New PickWave {1} created", pickslips.Length, bucketId));
                    }

                    _service.Value.AddPickslipsToWave(bucketId.Value, pickslips);
                    trans.Commit();
                    AddStatusMessage(string.Format("{0} pickslips have been added to PickWave {1}", pickslips.Length, bucketId));
                }
            }
            return RedirectToAction(this.Actions.PickslipList(customerId, rowDimIndex, rowDimVal, colDimIndex, colDimVal, vwhId, bucketId));
        }

        protected override string ManagerRoleName
        {
            get
            {
                return ROLE_WAVE_MANAGER;
            }
        }
    }
}
