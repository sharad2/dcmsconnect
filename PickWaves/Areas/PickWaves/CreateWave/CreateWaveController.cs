using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.ViewModels;
using EclipseLibrary.Mvc.Controllers;
using System;
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

        private CreateWaveService _service;

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (_service == null)
            {
                _service = new CreateWaveService(this.HttpContext.Trace,
                    HttpContext.User.IsInRole(ROLE_WAVE_MANAGER) ? HttpContext.User.Identity.Name : string.Empty,
                    Request.UserHostName ?? Request.UserHostAddress
                    );
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
        private bool PopulatePickslipMatrixPartialModel(PickslipMatrixPartialViewModel model)
        {
            //model.CustomerId = customerId;
            PickslipDimension pdimRow;

            if (model.RowDimIndex.HasValue)
            {
                pdimRow = (PickslipDimension)Enum.Parse(typeof(PickslipDimension), model.RowDimIndex.ToString());
            }
            else
            {
                pdimRow = PickslipDimension.CustomerDcCancelDate;
            }

            PickslipDimension pdimCol;
            if (model.ColDimIndex.HasValue)
            {
                pdimCol = (PickslipDimension)Enum.Parse(typeof(PickslipDimension), model.ColDimIndex.ToString());
            }
            else
            {
                pdimCol = PickslipDimension.CustomerDc;
            }


            model.VwhList = (from item in _service.GetVWhListOfCustomer(model.CustomerId)
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

            var summary = _service.GetOrderSummary(model.CustomerId, model.VwhId, pdimRow, pdimCol);

            if (summary.CountValuesPerDimension == null)
            {
                // No more pickslips which can be added to a bucket. The dimension matrix will be empty.
                // This happens when the last set of pickslips has been added to the bucket.
                return false;
            }
            var requery = false;
            if (summary.CountValuesPerDimension[pdimRow] == 0)
            {
                pdimRow = summary.CountValuesPerDimension.First(p => p.Value > 0).Key;
                requery = true;
            }

            if (summary.CountValuesPerDimension[pdimCol] == 0)
            {
                pdimCol = summary.CountValuesPerDimension.First(p => p.Value > 0).Key;
                requery = true;
            }

            if (requery)
            {
                summary = _service.GetOrderSummary(model.CustomerId, model.VwhId, pdimRow, pdimCol);
            }

            model.RowDimDisplayName = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()[pdimRow].Name;
            model.ColDimDisplayName = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()[pdimCol].Name;



            const int MAX_COL_DIMENSIONS = 30;

            model.RowDimensionList = (from kvp in PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()
                                      let count = summary.CountValuesPerDimension[kvp.Key]
                                      select new SelectListItem
                                      {
                                          Value = count == 0 ? "" : ((int)(kvp.Key)).ToString(),
                                          Text = string.Format("{0} ({1:N0})", kvp.Value.Name, count)
                                      }).OrderBy(p => p.Text).ToArray();

            // Dimensions which have too many distinct values are not displayed as column dimensions
            model.ColDimensionList = (from kvp in PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()
                                      let count = summary.CountValuesPerDimension[kvp.Key]
                                      where count <= MAX_COL_DIMENSIONS
                                      select new SelectListItem
                                      {
                                          Value = count > MAX_COL_DIMENSIONS || count == 0 ? "" : ((int)(kvp.Key)).ToString(),
                                          Text = string.Format("{0} ({1:N0})", kvp.Value.Name, count)
                                      }).OrderBy(p => p.Text).ToArray();

            model.RowDimIndex = (int)pdimRow;
            model.ColDimIndex = (int)pdimCol;

            model.Rows = (from rowVal in summary.AllValues.RowValues
                          let row = summary.AllValues.GetRow(rowVal)
                          select new RowDimensionModel
                          {
                              PickslipCounts = row.ToDictionary(p => FormatDimensionValue(p.Key), p => p.Value.PickslipCount),
                              OrderedPieces = row.ToDictionary(p => FormatDimensionValue(p.Key), p => p.Value.OrderedPieces),
                              DimensionValue = FormatDimensionValue(rowVal)
                          }).ToArray();

            model.ColDimensionValues = summary.AllValues.ColValues.Select(p => FormatDimensionValue(p)).ToArray();


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
        public virtual ActionResult Index(string customerId, int? rowDimIndex = null, int? colDimIndex = null, string vwhId = null, string pullAreaId = null, string pitchAreaId = null,
            int? lastBucketId = null)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new NotImplementedException();
            }

            var model = new IndexViewModel();
            //Showing only those area where order of customer are available.
            var areas = _service.GetAreasForCustomer(customerId);

            model.PullAreas = (from area in areas
                                where area.AreaType == BucketActivityType.Pulling && area.CountSku > 0
                                orderby area.CountSku descending
                                select new SelectListItem
                                {
                                    Text = string.Format("{0}: {1} ({2:N0}% SKUs available)", area.ShortName ?? area.AreaId, area.Description, area.CountOrderedSku == 0 ? 0 : area.CountSku * 100 / area.CountOrderedSku),
                                    Value = area.AreaId
                                }).ToList();

            model.PitchAreas = (from area in areas
                                 where area.AreaType == BucketActivityType.Pitching && area.CountSku > 0
                                 orderby area.CountSku descending
                                 select new SelectListItem
                                 {
                                     Text = string.Format("{0}: {1} ({2:N0}% SKUs assigned.)", area.ShortName ?? area.AreaId, area.Description, area.CountOrderedSku == 0 ? 0 : area.CountSku * 100 / area.CountOrderedSku),
                                     Value = area.AreaId
                                 }).ToList();

            if (model.PullAreas.Count == 0 && areas.Where(p => p.AreaType == BucketActivityType.Pulling).Count() > 0)
            {
                // Pull areas exist but none of them have SKUs available
                model.PullAreas.Add(new SelectListItem
                {
                    Text = "(Ordered SKUs are not available in any Pull Area)",
                    Value = "",
                    Selected = true
                });
            }

            if (model.PitchAreas.Count == 0 && areas.Where(p => p.AreaType == BucketActivityType.Pitching).Count() > 0)
            {
                // Pitch areas exist but none of them have SKUs assigned
                model.PitchAreas.Add(new SelectListItem
                {
                    Text = "(Ordered SKUs are not assigned in any Pitch Area)",
                    Value = "",
                    Selected = true
                });
            }


            #region Manage Cookie
            //  SelectedDimension : If null, then a reasonable default is used. The default is cookie => factory default
            //  If non null, then it is used and written to a cookie.
            var cookie = Request.Cookies[COOKIE_PICKWAVE];

            if (rowDimIndex == null || colDimIndex == null)
            {
                // Read cookie
                int dimRowIndex, dimColIndex;
                if (cookie != null && cookie[COOKIE_ROW_DIMENSION] != null && int.TryParse(cookie[COOKIE_ROW_DIMENSION], out dimRowIndex) &&
                    int.TryParse(cookie[COOKIE_COL_DIMENSION], out dimColIndex))
                {
                    model.RowDimIndex = dimRowIndex;
                    model.ColDimIndex = dimColIndex;
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
            model.RowDimIndex = rowDimIndex;
            model.ColDimIndex = colDimIndex;

            // Make sure that selected row and dimension are within the bounds of their respective drop downs
            if (!PopulatePickslipMatrixPartialModel(model))
            {
                var nopickslip = new IndexNoPickslipsViewModel
                {
                    BucketId = lastBucketId,
                    CustomerId = customerId
                };
                return View(Views.IndexNoPickslips, nopickslip);
            }


            var cust = _service.GetCustomer(model.CustomerId);
            model.CustomerName = cust == null ? model.CustomerId : cust.Name;

            // If Bucket is created.
            if (lastBucketId.HasValue)
            {
                // Retrive some information of bucket.
                model.LastBucketId = lastBucketId;
                var bucket = _service.GetEditableBucket(model.LastBucketId.Value);
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
        public virtual ActionResult AddPickslipsOfDim(IndexViewModel model)
        {
            if (model.ColDimVal != null || model.RowDimVal != null)
            {
                model.ColDimVal = model.ColDimVal.Trim();
                model.RowDimVal = model.RowDimVal.Trim();
            }

            var pdimRow = (PickslipDimension)Enum.Parse(typeof(PickslipDimension), model.RowDimIndex.ToString());
            var pdimCol = (PickslipDimension)Enum.Parse(typeof(PickslipDimension), model.ColDimIndex.ToString());

            if (ModelState.IsValid)
            {
                // If bucket is not created.create it first.
                if (!model.LastBucketId.HasValue)
                {
                    var bucket = new PickWaveEditable
                    {
                        PriorityId = 1,   // Default priority
                        QuickPitch = model.QuickPitch
                    };

                    // Give pull area if user wants to pulled cartons.
                    bucket.PullAreaId = model.PullAreaId;

                    // Give pitch area if user wants to pitched pieces.
                    bucket.PitchAreaId = model.PitchAreaId;

                    // Update Bucket name default. it change when pickslip is added to this bucket.
                    bucket.BucketName = "Bucket";

                    if (string.IsNullOrWhiteSpace(model.PullAreaId))
                    {
                        // This bucket is not pulling buccket.
                        bucket.PullingBucket = null;
                    }
                    else
                    {
                        if (model.RequiredBoxExpediting)
                        {
                            // Bucket is pull and Required Box Expediting (ADREPPWSS)
                            bucket.PullingBucket = "N";
                        }
                        else
                        {
                            // Pulling bucket , ADRE bucket
                            bucket.PullingBucket = "Y";
                        }
                    }
                    //Now Create bucket
                    model.LastBucketId = _service.CreateWave(bucket, model.CustomerId, pdimRow, model.RowDimVal, pdimCol, model.ColDimVal, model.VwhId);
                    AddStatusMessage(string.Format("Pick Wave {0} created.", model.LastBucketId));
                }
                else
                {
                    // Add pickslip to bucket 
                    _service.AddPickslipsPerDim(model.LastBucketId.Value, model.CustomerId, pdimRow, model.RowDimVal, pdimCol, model.ColDimVal, model.VwhId);
                    AddStatusMessage(string.Format("Pickslips added to Pick Wave {0}", model.LastBucketId));
                }
            }
            return RedirectToAction(Actions.Index(model.CustomerId, model.RowDimIndex, model.ColDimIndex,
                model.VwhId, model.PullAreaId, model.PitchAreaId, model.LastBucketId));
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
            int rowDimIndex,
            [Required]
            string rowDimVal,
            int colDimIndex,
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
            var model2 = new PickslipListViewModel();
            model2.CustomerId = customerId;

            model2.RowDimIndex = rowDimIndex;
            model2.ColDimIndex = colDimIndex;

            PickslipDimension pdimRow;

            if (model2.RowDimIndex.HasValue)
            {
                pdimRow = (PickslipDimension)model2.RowDimIndex.Value;
            }
            else
            {
                pdimRow = PickslipDimension.CustomerDcCancelDate;
            }

            PickslipDimension pdimCol;
            if (model2.ColDimIndex.HasValue)
            {
                pdimCol = (PickslipDimension)model2.ColDimIndex.Value;
            }
            else
            {
                pdimCol = PickslipDimension.CustomerDc;
            }
            model2.VwhId = vwhId;
            model2.RowDimVal = rowDimVal;
            model2.ColDimVal = colDimVal;
            // Pickslip list of passed dimension.
            var pickslips = _service.GetPickslipList(model2.CustomerId, model2.VwhId, pdimRow, model2.RowDimVal, pdimCol, model2.ColDimVal);
            model2.PickslipList = (from pickslip in pickslips
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
            model2.ManagerRoleName = ROLE_WAVE_MANAGER;

            model2.RowDimDisplayName = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()[pdimRow].Name;
            model2.ColDimDisplayName = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()[pdimCol].Name;
            model2.RowDimVal = string.IsNullOrEmpty(model2.RowDimVal) ? RowDimensionModel.NULL_DIMENSION_VALUE : model2.RowDimVal;
            model2.CustomerName = (_service.GetCustomer(model2.CustomerId) == null ? "" : _service.GetCustomer(model2.CustomerId).Name);

            model2.BucketId = bucketId;

            if (model2.BucketId.HasValue)
            {
                var bucket = _service.GetBucket(model2.BucketId.Value);
                model2.Bucket = new BucketModel(bucket);
            }
            else
            {
                //model.Bucket = new BucketModel();
            }
            return View(Views.PickslipList, model2);
        }

        /// <summary>
        /// Add passed pickslips to passed bucket.
        /// </summary>
        /// <param name="model"> </param>
        /// <returns></returns>
        [HttpPost]
        [Route("addps")]
        public virtual ActionResult AddPickslipsToBucket(PickslipListViewModel model)
        {
            if (model.BucketId == null)
            {
                throw new ArgumentNullException("model.BucketId");
            }
            if (model.SelectedPickslip.Count == 0)
            {
                AddStatusMessage("Please select pickslip.");
            }
            else
            {
                _service.AddPickslipsToWave(model.BucketId.Value, model.SelectedPickslip);
                AddStatusMessage(string.Format("{0} pickslips have been added to PickWave {1}", model.SelectedPickslip.Count, model.BucketId));
            }
            return RedirectToAction(this.Actions.PickslipList(model.CustomerId, model.RowDimIndex ?? 0, model.RowDimVal, model.ColDimIndex ?? 0, model.ColDimVal, model.VwhId, model.BucketId));
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
