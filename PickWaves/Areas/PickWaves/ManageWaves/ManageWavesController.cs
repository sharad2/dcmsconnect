using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.ViewModels;
using EclipseLibrary.Mvc.Controllers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    /// <summary>
    /// This controller requires authentication. Some actions which are decorated with [AllowAnonymous] can be accessed anonymously.
    /// </summary>
    [AuthorizeEx("Managing Pick Waves requires role {0}", Roles = ROLE_WAVE_MANAGER)]
    [RoutePrefix("manage")]
    public partial class ManageWavesController : PickWavesControllerBase
    {
        #region Intialization

        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public ManageWavesController()
        {

        }

        private Lazy<ManageWavesService> _service;

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (_service == null)
            {
                _service = new Lazy<ManageWavesService>(() => new ManageWavesService(this.HttpContext.Trace,
                    HttpContext.User.IsInRole(ROLE_WAVE_MANAGER) ? HttpContext.User.Identity.Name : string.Empty,
                    HttpContext.Request.UserHostName ?? HttpContext.Request.UserHostAddress
                    ));
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
        /// Used by views
        /// </summary>
        public static string BucketSummaryReportUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_140/R140_02.aspx";
            }
        }

        /// <summary>
        /// Showing the bucket list of passed customer and status. userName is optional. If passed, then waves created by the passed user are displayed.
        /// </summary>
        /// <param name="model">
        /// CustomerId,BucketStatus
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// customerId is required. If not passed, we silently redirect to home page
        /// If customerId is garbage and the bucket list is empty, then also we redirect to home page.
        /// bucketState is optional and defaults to InProgress
        /// </remarks>
        [AllowAnonymous]
        [Route]
        public virtual ActionResult Index(string customerId, string userName, ProgressStage? bucketState)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                // Should never happen. Redirect to home page
                return RedirectToAction(MVC_PickWaves.PickWaves.Home.Index());
            }
            if (bucketState == null)
            {
                // By default show in progress pick waves
                bucketState = ProgressStage.InProgress;
            }
            var buckets = _service.Value.GetBuckets(customerId, bucketState.Value, userName);

            var model = new IndexViewModel
            {
                CustomerId = customerId,
                BucketState = bucketState.Value,
                UserName = userName
            };
            // Null DC Cancel dates display last
            model.Buckets = (from bucket in buckets.Select(p => new BucketModel(p, _service.Value.GetCustomerName(customerId), BucketModelFlags.Default))
                             orderby bucket.PriorityId descending, bucket.DcCancelDateRange.From ?? DateTime.MaxValue, bucket.PercentPiecesComplete descending
                             select bucket).ToList();


            //model.CustomerName = _service.Value.GetCustomerName(model.CustomerId);

            if (string.IsNullOrWhiteSpace(model.CustomerName) && model.Buckets.Count == 0)
            {
                // This must be a garbage customer. Redirect to home page
                ModelState.AddModelError("", string.Format("Customer {0} not recognized", customerId));
                return RedirectToAction(MVC_PickWaves.PickWaves.Home.Customers());
            }

            return View(Views.Index, model);
        }

        #region Wave Viewer
        ///// <summary>
        ///// Displays details of passed bucket id
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[AllowAnonymous]
        //[Route("wavepickslips")]
        //public virtual ActionResult WavePickslips(int bucketId)
        //{
        //    var bucket = _service.Value.GetBucket(bucketId);
        //    if (bucket == null)
        //    {
        //        // Unreasonable bucket id
        //        //ModelState.AddModelError("", string.Format("Pick Wave {0} is deleted", model.Bucket.BucketId));
        //        return RedirectToAction(MVC_PickWaves.PickWaves.Home.Customers());
        //    }
        //    var model = new WaveViewModel
        //    {
        //        //HighlightedActions = nextAction
        //    };
        //    model.Bucket = new BucketModel(bucket)
        //    {
        //        CustomerName = _service.Value.GetCustomerName(bucket.MaxCustomerId)
        //    };

        //    // If Bucket is pulling bucket and value of PullingBucket is N. then Bucket Required Box Expediting
        //    //if (!string.IsNullOrWhiteSpace(bucket.PullingBucket) && bucket.PullingBucket == "N")
        //    //{
        //    //    model.Bucket.RequiredBoxExpediting = true;
        //    //}

        //    return View(this.Views.WavePickslips, model);
        //}

        /// <summary>
        /// Ajax call.
        /// Showing pickslip list of bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="stateFilter"> </param>
        /// <param name="activityFilter"> </param>
        /// <returns></returns>
        /// <remarks>
        /// For ease of debugging, prevent caching of the results
        /// </remarks>
        [AllowAnonymous]
        [Route("wavesku")]
        public virtual ActionResult WaveSkus(int bucketId)
        {
            var skuList = _service.Value.GetBucketSkuList(bucketId);
            var query = (from item in skuList
                         select new
                         {
                             BucketSku = item,
                             Activities = item.Activities.Select(p => new BucketActivityModel(p))
                               .Where(p => p.PiecesComplete > 0 || p.PiecesIncomplete > 0),
                             Areas = item.BucketSkuInAreas.Select(p => p.InventoryArea)
                         }).ToList();

            var allAreas = (from sku in skuList
                            from area in sku.BucketSkuInAreas
                            where !string.IsNullOrWhiteSpace(area.InventoryArea.AreaId)
                            select new InventoryAreaModel(area.InventoryArea)
                            ).Distinct(InventoryAreaModelComparer.Instance).ToList();

            var model = new WaveSkuListModel
            {
                BucketSkuList = (from sku in query
                                 select new BucketSkuModel
                                 {
                                     Style = sku.BucketSku.Sku.Style,
                                     Color = sku.BucketSku.Sku.Color,
                                     Dimension = sku.BucketSku.Sku.Dimension,
                                     SkuSize = sku.BucketSku.Sku.SkuSize,
                                     UpcCode = sku.BucketSku.Sku.UpcCode,
                                     SkuId = sku.BucketSku.Sku.SkuId,
                                     VwhId = sku.BucketSku.Sku.VwhId,
                                     VolumePerDozen = sku.BucketSku.Sku.VolumePerDozen,
                                     WeightPerDozen = sku.BucketSku.Sku.WeightPerDozen,
                                     OrderedPieces = sku.BucketSku.QuantityOrdered,
                                     InventoryByArea = (from area in allAreas
                                                        join item in sku.BucketSku.BucketSkuInAreas on area.AreaId equals item.InventoryArea.AreaId into gj
                                                        from subitem in gj.DefaultIfEmpty()
                                                        select new BucketSkuAreaModel
                                                        {
                                                            AreaId = area.AreaId,
                                                            ShortName = area.ShortName,
                                                            BuildingId = area.BuildingId,
                                                            Description = area.Description,
                                                            BestLocationId = subitem == null ? null : subitem.BestLocationId,
                                                            InventoryPieces = subitem == null || subitem.InventoryPieces == 0 ? (int?)null : subitem.InventoryPieces,
                                                            PiecesAtBestLocation = subitem == null ? (int?)null : subitem.PiecesAtBestLocation,
                                                            //QuantityInSmallestCarton = subitem == null ? (int?)null : subitem.PiecesInSmallestCarton
                                                        }).ToList(),
                                     Activities = sku.Activities.ToList()
                                 })
                                 .OrderBy(p => p.PercentCurrentPieces)
                                 .ThenBy(p => p.Style)
                                 .ThenBy(p => p.Color)
                                 .ThenBy(p => p.Dimension)
                                 .ThenBy(p => p.SkuSize)
                                 .ToArray(),
                AllAreas = allAreas,
                BucketId = bucketId,
            };

            return PartialView(this.Views._waveSkusPartial, model);
        }

        /// <summary>
        /// Ajax call.
        /// Showing boxes list of bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="stateFilter">You can pass multiple flags. Completed => Includes partially completed boxes. 
        /// InProgress => All unverified boxes.
        /// Note that non empty unverified boxes are returned both for Completed and InProgress
        /// </param>
        /// <param name="activityFilter">Pulling or pitching</param>
        /// <returns></returns>
        /// <remarks></remarks>     
        [AllowAnonymous]
        [Route("waveboxes")]
        public virtual ActionResult WaveBoxes(int bucketId)
        {
            var model = new WaveBoxListModel
             {
                 BucketId = bucketId,
                 //StateFilter = stateFilter,
                 //ActivityFilter = activityFilter,
                 BoxesList = (from box in _service.Value.GetBucketBoxes(bucketId)
                              let routeBox = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchUcc1]
                              let routePickslip = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchPickslip1]
                              let routeCarton = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchCarton1]
                              select new BoxModel
                              {
                                  Ucc128Id = box.Ucc128Id,
                                  AreaId = box.AreaId,
                                  VerifyDate = box.VerifyDate,
                                  ExpectedPieces = box.ExpectedPieces,
                                  CurrentPieces = box.CurrentPieces,
                                  CartonId = box.CartonId,
                                  CancelDate = box.CancelDate,
                                  MaxPitchingEndDate = box.PitchingEndDate,
                                  PickslipId = box.PickslipId,
                                  CreatedBy = box.CreatedBy,
                                  CreatedDate = box.CreatedDate,
                                  VWhId = box.VWhId,
                                  UrlInquiryBox = routeBox == null ? null : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchUcc1, new
                                  {
                                      id = box.Ucc128Id
                                  }),
                                  UrlInquiryPickslip = routePickslip == null ? null : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchPickslip1, new
                                    {
                                        id = box.PickslipId
                                    }),
                                  UrlInquiryCarton = routeCarton == null ? null : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchCarton1, new
                                  {
                                      id = box.CartonId
                                  })
                              }).ToArray()
             };
            return PartialView(this.Views._waveBoxesPartial, model);
        }

        /// <summary>
        /// This method is used to freeze a bucket.
        /// </summary>
        /// <param name="bucketId"> </param>
        /// <param name="freeze"> </param>
        /// <param name="displayEditable">Whether we should redirect to ediateble wave or read only wave</param>
        /// <returns></returns>
        [HttpPost]
        [Route("freeze")]
        public virtual ActionResult FreezeBucket(int bucketId)
        {
            try
            {
                using (var trans = _service.Value.BeginTransaction())
                {
                    _service.Value.FreezePickWave(bucketId, trans);
                }
                // user has frozen a bucket.
                AddStatusMessage(string.Format("Pick wave {0} has been frozen.", bucketId));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return RedirectToAction(this.Actions.WavePickslips(bucketId));
        }

        /// <summary>
        /// Ajax call.
        /// Showing pickslip list of bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("wavepickslip")]
        public virtual ActionResult WavePickslips(int bucketId)
        {
            var bucket = _service.Value.GetBucket(bucketId);
            if (bucket == null)
            {
                // Unreasonable bucket id
                //ModelState.AddModelError("", string.Format("Pick Wave {0} is deleted", model.Bucket.BucketId));
                return RedirectToAction(MVC_PickWaves.PickWaves.Home.Customers());
            }
            //model.Bucket = new BucketModel(bucket)
            //{
            //    CustomerName = _service.Value.GetCustomerName(bucket.MaxCustomerId)
            //};
            var pickslips = _service.Value.GetBucketPickslip(bucketId);

            var model = new WavePickslipsViewModel
                {
                    Bucket = new BucketModel(bucket, _service.Value.GetCustomerName(bucket.MaxCustomerId), BucketModelFlags.HideViewerLink | BucketModelFlags.ShowEditMenu),
                    PickslipList = (from pickslip in pickslips
                                    let routePickslip = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchPickslip1]
                                    let routePo = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchPo3]
                                    select new ManageWavesPickslipModel(pickslip)
                                        {
                                            UrlInquiryPickslip = routePickslip == null ? null : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchPickslip1, new
                                            {
                                                id = pickslip.PickslipId
                                            }),
                                            UrlInquiryPurchaseOrder = routePo == null ? null : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchPo3, new
                                            {
                                                id = pickslip.PurchaseOrder,
                                                pk1 = pickslip.CustomerId,
                                                pk2 = pickslip.Iteration
                                            })
                                        }).OrderBy(p => p.PercentCurrentPieces)
                                          .ThenByDescending(p => p.OrderedPieces).ToList(),

                };

            //if (pickslips.Count > 0)
            //{
            //    var ps = pickslips.First();
            //    model.CustomerId = ps.CustomerId;
            //    model.BucketId = ps.BucketId;
            //    model.IsFrozenBucket = ps.IsFrozenBucket;
            //}
            return View(this.Views.WavePickslips, model);
        }



        [HttpPost]
        [Route("freezeedit")]
        public virtual ActionResult FreezeAndEditBucket(int bucketId)
        {
            try
            {
                using (var trans = _service.Value.BeginTransaction())
                {
                    _service.Value.FreezePickWave(bucketId, trans);
                }
                // user has frozen a bucket.
                AddStatusMessage(string.Format("Pick wave {0} has been frozen.", bucketId));
                return RedirectToAction(this.Actions.WaveEditor(bucketId));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                // In case of error, show the wave viewer page
                return RedirectToAction(this.Actions.WavePickslips(bucketId));
            }

        }

        [HttpPost]
        [Route("unfreeze")]
        public virtual ActionResult UnfreezeBucket(int bucketId)
        {
            try
            {
                using (var trans = _service.Value.BeginTransaction())
                {
                    _service.Value.UnfreezePickWave(bucketId, trans);
                    trans.Commit();
                }
                // user has frozen a bucket.
                AddStatusMessage(string.Format("Pick wave {0} is no longer frozen.", bucketId));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return RedirectToAction(this.Actions.WavePickslips(bucketId));
        }
        #endregion

        #region Wave Editor

        [Route("editor")]
        public virtual ActionResult WaveEditor(int bucketId)
        {
            var bucket = _service.Value.GetEditableBucket(bucketId);
            if (bucket == null)
            {
                // Unreasonable bucket id
                ModelState.AddModelError("", string.Format("Unreasonable Pick Wave {0}", bucketId));
                return RedirectToAction(MVC_PickWaves.PickWaves.Home.Customers());
            }
            if (!bucket.IsFrozen)
            {
                // bucket is not freeze,freeze it before attempting to edit it.
                ModelState.AddModelError("", "Please freeze the pick wave before attempting to edit it");
                return RedirectToAction(Actions.WavePickslips(bucketId));
            }

            //var bucketAreas = _service.Value.GetBucketAreas(bucketId);
            var model = new WaveEditorViewModel(bucket);
            PopulateWaveEditorViewModel(model);
            //{
            //    // Show only those areas which have some SKUs available
            //    PullAreaList = (from area in bucketAreas
            //                    where area.AreaType == BucketActivityType.Pulling && area.CountOrderedSku.HasValue && area.CountOrderedSku.Value > 0 &&
            //                        area.CountSku.HasValue && area.CountSku > 0
            //                    orderby area.CountSku descending
            //                    let pctSkuAvailable = area.CountSku.Value * 100.0 / (double)area.CountOrderedSku.Value
            //                    select new SelectListItem
            //                    {
            //                        Text = string.Format("{0}: {1} ({2:N0}% SKUs available)", area.ShortName ?? area.AreaId, area.Description,
            //                            area.CountOrderedSku == 0 ? 0 : area.CountSku * 100 / area.CountOrderedSku),
            //                        Value = area.AreaId,
            //                        Selected = area.AreaId == bucket.PullAreaId
            //                    }).ToList(),
            //    PitchAreaList = (from area in bucketAreas
            //                     where area.AreaType == BucketActivityType.Pitching && area.CountOrderedSku.HasValue && area.CountOrderedSku.Value > 0 &&
            //                        area.CountSku.HasValue && area.CountSku > 0
            //                     orderby area.CountSku descending
            //                     let pctSkuAssigned = area.CountSku.Value * 100.0 / (double)area.CountOrderedSku.Value
            //                     select new SelectListItem
            //                     {
            //                         Text = string.Format("{0}: {1} ({2:N0}% SKUs assigned.)", area.ShortName ?? area.AreaId, area.Description, pctSkuAssigned),
            //                         Value = area.AreaId,
            //                         Selected = area.AreaId == bucket.PitchAreaId
            //                     }).ToList(),
            //    CustomerName = _service.Value.GetCustomerName(bucket.CustomerId)
            //};

            return View(Views.WaveEditor, model);
        }

        /// <summary>
        /// Update passed bucket
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("updatewave")]
        public virtual ActionResult UpdateWave(WaveEditorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateWaveEditorViewModel(model);
                return View(Views.WaveEditor, model);
            }
            var bucket = new BucketEditable
            {
                //BucketId = model.BucketId,
                BucketName = model.BucketName,
                //PriorityId = model.PriorityId,
                BucketComment = model.BucketComment,
                QuickPitch = model.QuickPitch,
                PitchLimit = model.PitchLimit.Value,
                RequireBoxExpediting = model.RequiredBoxExpediting,
                PullAreaId = model.PullAreaId,
                PitchAreaId = model.PitchAreaId
            };

            try
            {
                using (var trans = _service.Value.BeginTransaction())
                {
                    _service.Value.UpdateWave(model.BucketId, bucket, trans);
                    if (model.UnfreezeWaveAfterSave)
                    {
                        //  if user says unfreeze bucket after editing.
                        _service.Value.UnfreezePickWave(model.BucketId, trans);
                    }

                    trans.Commit();
                }
                AddStatusMessage(string.Format("Pick Wave {0} updated.", model.BucketId));
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", "Pick wave could not be updated. Please review the error and try again");
                ModelState.AddModelError("", ex.Message);
                // return RedirectToAction(this.Actions.EditableWave(model.BucketId, SuggestedNextActionType.NotSet));
                return RedirectToAction(this.Actions.WavePickslips(model.BucketId));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                //return RedirectToAction(this.Actions.EditableWave(model.BucketId, SuggestedNextActionType.NotSet));
                return RedirectToAction(this.Actions.WavePickslips(model.BucketId));
            }

            return RedirectToAction(this.Actions.WavePickslips(model.BucketId));
        }

        /// <summary>
        /// Populates the area lists in the passed model. The model must at least contain the bucket id. If customer id is in the model, the customer name is populated as well.
        /// You should also pass the Customer Id within the model. It is needed if model validation fails
        /// </summary>
        /// <param name="?"></param>
        private void PopulateWaveEditorViewModel(WaveEditorViewModel model)
        {
            var bucketAreas = _service.Value.GetBucketAreas(model.BucketId);

            var groups = bucketAreas.Select(p => p.BuildingId).Distinct().Select(p => new SelectListGroup
            {
                Name = p
            }).ToDictionary(p => p.Name);

            var allAreas = from area in bucketAreas
                           where area.CountOrderedSku.HasValue && area.CountOrderedSku.Value > 0 &&
                               area.CountSku.HasValue && area.CountSku > 0
                           orderby area.BuildingId, area.CountSku descending
                           let pctSkuAvailable = area.CountSku.Value * 100.0 / (double)area.CountOrderedSku.Value
                           select new
                           {
                               AreaId = area.AreaId,
                               AreaType = area.AreaType,
                               ShortName = area.ShortName ?? area.AreaId,
                               Description = area.Description,
                               PctSkuAvailable = area.CountSku.Value * 100.0 / (double)area.CountOrderedSku.Value,
                               Text = string.Format("{0}: {1} ({2:N0}% SKUs available)", area.ShortName ?? area.AreaId, area.Description,
                                   area.CountOrderedSku == 0 ? 0 : area.CountSku * 100 / area.CountOrderedSku),
                               Value = area.AreaId,
                               Group = groups[area.BuildingId]
                           };
            model.PullAreaList = (from area in allAreas
                                  where area.AreaType == BucketActivityType.Pulling
                                  select new SelectListItem
                                   {
                                       Text = string.Format("{0}: {1} ({2:N0}% SKUs available)", area.ShortName, area.Description, area.PctSkuAvailable),
                                       Value = area.AreaId,
                                       Selected = area.AreaId == model.PullAreaId,
                                       Group = area.Group
                                   }).ToList();

            model.PitchAreaList = (from area in allAreas
                                   where area.AreaType == BucketActivityType.Pitching
                                   select new SelectListItem
                                   {
                                       Text = string.Format("{0}: {1} ({2:N0}% SKUs assigned.)", area.ShortName ?? area.AreaId, area.Description, area.PctSkuAvailable),
                                       Value = area.AreaId,
                                       Selected = area.AreaId == model.PitchAreaId,
                                       Group = area.Group
                                   }).ToList();

            // Show only those areas which have some SKUs available
            //model.PullAreaList = (from area in bucketAreas
            //                      where area.AreaType == BucketActivityType.Pulling && area.CountOrderedSku.HasValue && area.CountOrderedSku.Value > 0 &&
            //                          area.CountSku.HasValue && area.CountSku > 0
            //                      orderby area.CountSku descending
            //                      let pctSkuAvailable = area.CountSku.Value * 100.0 / (double)area.CountOrderedSku.Value
            //                      select new SelectListItem
            //                      {
            //                          Text = string.Format("{0}: {1} ({2:N0}% SKUs available)", area.ShortName ?? area.AreaId, area.Description,
            //                              area.CountOrderedSku == 0 ? 0 : area.CountSku * 100 / area.CountOrderedSku),
            //                          Value = area.AreaId,
            //                          Selected = area.AreaId == model.PullAreaId,
            //                          Group = groups[area.BuildingId]
            //                      }).ToList();
            //model.PitchAreaList = (from area in bucketAreas
            //                       where area.AreaType == BucketActivityType.Pitching && area.CountOrderedSku.HasValue && area.CountOrderedSku.Value > 0 &&
            //                          area.CountSku.HasValue && area.CountSku > 0
            //                       orderby area.CountSku descending
            //                       let pctSkuAssigned = area.CountSku.Value * 100.0 / (double)area.CountOrderedSku.Value
            //                       select new SelectListItem
            //                       {
            //                           Text = string.Format("{0}: {1} ({2:N0}% SKUs assigned.)", area.ShortName ?? area.AreaId, area.Description, pctSkuAssigned),
            //                           Value = area.AreaId,
            //                           Selected = area.AreaId == model.PitchAreaId
            //                       }).ToList();
            model.CustomerName = _service.Value.GetCustomerName(model.CustomerId);
        }
        #endregion

        /// <summary>
        /// Increase priority.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns>Returns the value of the updated priority</returns>
        [HttpPost]
        [Route("priority")]
        public virtual ActionResult IncrementPriority(int bucketId)
        {
            var priority = _service.Value.IncrementPriority(bucketId);
            return Json(priority);
        }

        /// <summary>
        /// Decrease priority.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns>Returns the value of the updated priority</returns>
        [HttpPost]
        [Route("decpriority")]
        public virtual ActionResult DecrementPriority(int bucketId)
        {
            var priority = _service.Value.DecrementPriority(bucketId);
            return Json(priority);
        }




        /// <summary>
        /// Remove passed pickslip from bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="pickslipId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("removeps")]
        public virtual ActionResult RemovePickslipFromBucket(int bucketId, long pickslipId)
        {
            try
            {
                _service.Value.RemovePickslipFromBucket(pickslipId, bucketId);
            }
            catch (DbException exception)
            {
                this.Response.StatusCode = 203;
                return Content(exception.Message);
            }
            return Content(string.Format("Pickslip {0} removed from wave {1}.", pickslipId, bucketId));
        }

        protected override string ManagerRoleName
        {
            get { return ROLE_WAVE_MANAGER; }
        }

        [HttpPost]
        [Route("cancelboxes")]
        public virtual ActionResult CancelBoxes(int bucketId, string[] boxes)
        {

            if (boxes == null || boxes.Length < 1)
            {
                ModelState.AddModelError("","You have not selected any Boxes to cancel, please select atleast 1 box to cancel.");
                return RedirectToAction(Actions.WavePickslips(bucketId));
            }
            try
            {
                _service.Value.CancelBoxes(boxes);
            }
            catch (DbException exception)
            {
                this.ModelState.AddModelError("", exception.InnerException);
            }           
            AddStatusMessage(string.Format("{0} boxes cancelled", boxes.Length));
            return RedirectToAction(Actions.WavePickslips(bucketId));
        }


    }
}
