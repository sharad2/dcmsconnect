using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository;
using DcmsMobile.PickWaves.Repository.CreateWave;
using DcmsMobile.PickWaves.ViewModels;
using DcmsMobile.PickWaves.ViewModels.CreateWave;
using EclipseLibrary.Mvc.Controllers;


namespace DcmsMobile.PickWaves.Areas.PickWaves.Controllers
{
    [AuthorizeEx("Managing Pick Waves requires role {0}", Roles = ROLE_WAVE_MANAGER)]
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
        /// <param name="selectedRowDimension"> </param>
        /// <param name="selectedColumnDimension"> </param>
        /// <returns></returns>
        private void PopulatePickslipMatrixPartialModel(PickslipMatrixPartialViewModel model, string customerId, int selectedRowDimension, int selectedColumnDimension)
        {
            model.CustomerId = customerId;
            model.RowDimIndex = selectedRowDimension;
            model.ColDimIndex = selectedColumnDimension;
            var pdimRow = (PickslipDimension)Enum.Parse(typeof(PickslipDimension), selectedRowDimension.ToString());
            var pdimCol = (PickslipDimension)Enum.Parse(typeof(PickslipDimension), selectedColumnDimension.ToString());

            model.RowDimDisplayName = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()[pdimRow].Name;
            model.ColDimDisplayName = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()[pdimCol].Name;
            model.VwhList = from item in _service.GetVWhListOfCustomer(customerId)
                            select new SelectListItem
                                {
                                    Text = item.VWhId + " : " + item.Description,
                                    Value = item.VWhId,
                                    Selected = item.VWhId == model.VwhId
                                };
            var orders = _service.GetOrderSummary(customerId, model.VwhId, pdimRow, pdimCol);

            if (orders.Any())
            {
                // TC1: When passed customer have some order.
                const int MAX_DIMENSIONS = 500;
                var first = orders.First();

                var query = (from kvp in PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()
                             let count = first.Counts[kvp.Key]
                             select new SelectListItem
                             {
                                 Value = count > MAX_DIMENSIONS ? "" : ((int)(kvp.Key)).ToString(),
                                 Text = string.Format("{0} ({1:N0})", kvp.Value.Name, count)
                             }).OrderBy(p => p.Text).ToArray();

                model.DimensionList = query;

                model.CustomerOrders = (from order in orders
                                        select new PickslipDimensionModel
                                        {
                                            Data = order.Data,
                                            DimensionValue = FormatDimensionValue(order.DimensionValue)
                                        }).ToArray();
            }
        }

        /// <summary>
        /// Showing list of dimension and their pickslip count for passed customer and dimension.
        /// </summary>
        /// <param name="model">
        /// Posted value
        /// SelectedDimension,CustomerId
        /// </param>       
        /// <returns></returns>
        public virtual ActionResult Index(IndexViewModel model)
        {
            PopulatePickslipMatrixPartialModel(model, model.CustomerId, model.RowDimIndex.Value, model.ColDimIndex.Value);

            var areas = _service.GetAreasForCustomer(model.CustomerId);

            model.PullAreaList = (from area in areas
                                  where area.AreaType == BucketActivityType.Pulling
                                  orderby area.CountSku descending
                                  select new CreateWaveAreaModel(area)).ToArray();
            if (string.IsNullOrWhiteSpace(model.PullAreaId) && model.PullAreaList.Count > 0)
            {
                // TC2: Shows best pull area for pulling
                model.PullAreaId = model.PullAreaList[0].AreaId;
            }

            model.PitchAreaList = (from area in areas
                                   where area.AreaType == BucketActivityType.Pitching
                                   orderby area.CountSku descending
                                   select new CreateWaveAreaModel(area)).ToArray();

            if (string.IsNullOrWhiteSpace(model.PitchAreaId) && model.PitchAreaList.Count > 0)
            {
                // TC3: Shows best pitch area for pitching
                model.PitchAreaId = model.PitchAreaList[0].AreaId;
            }

            #region Manage Cookie
            //  SelectedDimension : If null, then a reasonable default is used. The default is cookie => factory default
            //  If non null, then it is used and written to a cookie.
            var cookie = Request.Cookies[COOKIE_PICKWAVE];

            if (model.RowDimIndex == null || model.ColDimIndex == null)
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
            model.CustomerName = (_service.GetCustomer(model.CustomerId) == null ? "" : _service.GetCustomer(model.CustomerId).Name);
            return View(Views.Index, model);
        }

        /// <summary>
        /// Ajax call : This method refreshes the pickslip list, when change dimension value.
        /// </summary>
        /// <param name="model">
        /// Posted value
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// Need to refresh the pickslip list every time after wave creation. i.e [OutputCache(Duration = 0)]
        /// </remarks>
        [OutputCache(Duration = 0)]
        public virtual ActionResult RefreshPickslipMatrix(PickslipMatrixPartialViewModel model)
        {
            PopulatePickslipMatrixPartialModel(model, model.CustomerId, model.RowDimIndex.Value, model.ColDimIndex.Value);
            var html = RenderPartialViewToString(this.Views._pickslipMatrixPartial, model);
            return Content(html);
        }

        /// <summary>
        /// Create a new bucket.
        /// </summary>
        /// <param name="model"> 
        /// Posted value: model.CreateBucket, model.CustomerId,model.SelectedDc, model.SelectedDimension,model.SelectedDimensionVal
        /// </param>
        /// <returns>
        /// </returns>
        [HttpPost]
        public virtual ActionResult CreatePickWave(IndexViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(MVC_PickWaves.PickWaves.CreateWave.Index(model));
            }
            var bucket = new Bucket
            {
                PriorityId = 1,   // Default priority
                RequireBoxExpediting = model.RequireBoxExpediting,
                IsFrozen = true
            };
            if (model.AllowPulling)
            {
                // TC4: Give pull area if user wants to pulled cartons.
                bucket.Activities[BucketActivityType.Pulling].Area.AreaId = model.PullAreaId;
            }
            if (model.AllowPitching)
            {
                // TC5: Give pitch area if user wants to pitched pieces.
                bucket.Activities[BucketActivityType.Pitching].Area.AreaId = model.PitchAreaId;
            }
            bucket.BucketName = "Bucket";
                //string.Format("{0}-{1} {2}/{3} {4}", model.CustomerId,
    // Helpers.PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()[pdimRow].ShortName,
    // model.RowDimVal,
    // Helpers.PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()[pdimCol].ShortName,
    // model.ColDimVal); // 'WC3 WM ' || ORDERS_REC.CUSTOMER_ORDER_ID || ORDERS_REC.dc_cancel_date,
            try
            {
                model.LastBucketId = _service.CreateWave(bucket);
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return RedirectToAction(this.Actions.Index(model));
        }

        [HttpPost]
        public virtual ActionResult AddPickslipsOfDim(IndexViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(MVC_PickWaves.PickWaves.CreateWave.Index(model));
            }

            var pdimRow = (PickslipDimension)Enum.Parse(typeof(PickslipDimension), model.RowDimIndex.ToString());
            var pdimCol = (PickslipDimension)Enum.Parse(typeof(PickslipDimension), model.ColDimIndex.ToString());
            try
            {
                _service.AddPickslipsPerDim(model.LastBucketId.Value, model.CustomerId, pdimRow, model.RowDimVal, pdimCol, model.ColDimVal);
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return RedirectToAction(this.Actions.Index(model));
        }

        /// <summary>
        /// Select dimension to add pickslip in passed bucket.
        /// </summary>
        /// <param name="model">
        /// Posted value : model.CustomerId, model.BucketId
        /// </param>
        /// <returns></returns>
        public virtual ActionResult PickslipListSelector(PickslipListSelectorViewModel model)
        {
            PopulatePickslipMatrixPartialModel(model, model.CustomerId, model.RowDimIndex.Value, model.ColDimIndex.Value);
            if (model.CustomerOrders.Count == 0)
            {
                AddStatusMessage(string.Format("No imported orders found for Customer {0}", model.CustomerId));
            }
            var bucket = _service.GetBucket(model.BucketId);
            model.Bucket = new BucketModel(bucket);
            return View(Views.PickslipListSelector, model);
        }

        /// <summary>
        /// Get pickslip list of passed criteria.
        /// </summary>
        /// <param name="model">
        /// model.CustomerId, model.CustomerDc, model.SelectedDimension, model.DimensionDisplayValue, model.addToBucketId
        /// </param
        /// <returns></returns>
        public virtual ActionResult PickslipList(PickslipListViewModel model)
        {
            if (string.IsNullOrEmpty(model.CustomerId))
            {
                throw new ArgumentNullException("customerId");
            }
            var pdimRow = (PickslipDimension)Enum.Parse(typeof(PickslipDimension), model.RowDimIndex.ToString());
            var pdimCol = (PickslipDimension)Enum.Parse(typeof(PickslipDimension), model.ColDimIndex.ToString());

            var pickslips = _service.GetPickslipList(model.CustomerId, pdimRow, model.RowDimVal, pdimCol, model.ColDimVal);
            model.PickslipList = (from pickslip in pickslips
                                  select new PickslipModel
                                  {
                                      PickslipId = pickslip.PickslipId,
                                      PurchaseOrder = pickslip.PurchaseOrder,
                                      VwhId = pickslip.VwhId,
                                      CancelDate = pickslip.CancelDate,
                                      PickslipImportDate = pickslip.PickslipImportDate,
                                      StartDate = pickslip.StartDate,
                                      CustomerDcId = pickslip.CustomerDcId,
                                      CustomerStoreId = pickslip.CustomerStoreId
                                  }).ToArray();
            model.ManagerRoleName = ROLE_WAVE_MANAGER;

            model.RowDimDisplayName = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()[pdimRow].Name;
            model.ColDimDisplayName = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DisplayAttribute>()[pdimCol].Name;
            model.RowDimVal = string.IsNullOrEmpty(model.RowDimVal) ? PickslipDimensionModel.NULL_DIMENSION_VALUE : model.RowDimVal;
            model.CustomerName = (_service.GetCustomer(model.CustomerId) == null ? "" : _service.GetCustomer(model.CustomerId).Name);
            var bucket = _service.GetBucket(model.BucketId);
            model.Bucket = new BucketModel(bucket);
            return View(Views.PickslipList, model);
        }

        /// <summary>
        /// Add passed pickslips to passed bucket.
        /// </summary>
        /// <param name="model"> </param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult AddPickslipsToBucket(PickslipListViewModel model)
        {
            if (model.BucketId == 0)
            {
                throw new ArgumentNullException("model.BucketId");
            }
            if (model.SelectedPickslip != null)
            {
                _service.AddPickslipsToWave(model.BucketId, model.SelectedPickslip);
                AddStatusMessage(string.Format("{0} pickslips have been added to PickWave {1}", model.SelectedPickslip.Count, model.BucketId));
            }
            return RedirectToAction(this.Actions.PickslipList(model));
        }

        protected override string ManagerRoleName
        {
            get { return ROLE_WAVE_MANAGER; }
        }
    }
}
