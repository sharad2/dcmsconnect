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
        /// Showing the bucket list of passed customer and status
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
            model.Buckets = (from bucket in buckets.Select(p => new BucketModel(p))
                             orderby bucket.PriorityId descending, bucket.DcCancelDateRange.From ?? DateTime.MaxValue, bucket.PercentPiecesComplete descending
                             select bucket).ToList();


            model.CustomerName = _service.Value.GetCustomerName(model.CustomerId);

            if (string.IsNullOrWhiteSpace(model.CustomerName) && model.Buckets.Count == 0)
            {
                // This must be a garbage customer. Redirect to home page
                ModelState.AddModelError("", string.Format("Customer {0} not recognized", customerId));
                return RedirectToAction(MVC_PickWaves.PickWaves.Home.Index());
            }

            return View(Views.Index, model);
        }

        #region Wave Viewer
        /// <summary>
        /// Honors the passed bucketid, displayEditable and HighlightedActions
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("wave")]
        public virtual ActionResult Wave(int bucketId)
        {
            var bucket = _service.Value.GetBucket(bucketId);
            if (bucket == null)
            {
                // Unreasonable bucket id
                //ModelState.AddModelError("", string.Format("Pick Wave {0} is deleted", model.Bucket.BucketId));
                return RedirectToAction(MVC_PickWaves.PickWaves.Home.Index());
            }
            var model = new WaveViewModel
            {
                //HighlightedActions = nextAction
            };
            model.Bucket = new BucketModel(bucket);

            // If Bucket is pulling bucket and value of PullingBucket is N. then Bucket Required Box Expediting
            if (!string.IsNullOrWhiteSpace(bucket.PullingBucket) && bucket.PullingBucket == "N")
            {
                model.Bucket.RequiredBoxExpediting = true;
            }
            if (!model.Bucket.IsFrozen)
            {
                //model.HighlightedActions = SuggestedNextActionType.UnfreezeOthers;
            }
            return View(this.Views.Wave, model);
        }

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
            var skuList = (from item in _service.Value.GetBucketSkuList(bucketId)
                           select new
                           {
                               BucketSku = item,
                               Activities = item.Activities.Select(p => new BucketActivityModel(p))
                                 .Where(p => p.PiecesComplete > 0 || p.PiecesIncomplete > 0),
                               Areas = item.BucketSkuInAreas.Select(p => p.InventoryArea)
                           }).ToArray();

            var allAreas = (from sku in skuList
                            from area in sku.Areas
                            where !string.IsNullOrWhiteSpace(area.AreaId)
                            select new InventoryAreaModel(area)
                            ).Distinct(InventoryAreaModelComparer.Instance).ToArray();

            var model = new WaveSkuListModel
            {
                BucketSkuList = (from sku in skuList
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
                                     IsAssignedSku = sku.BucketSku.IsPitchingBucket ? sku.BucketSku.Sku.IsAssignedSku : true,
                                     InventoryByArea = (from area in allAreas
                                                        join item in sku.BucketSku.BucketSkuInAreas on area.AreaId equals item.InventoryArea.AreaId into gj
                                                        from subitem in gj.DefaultIfEmpty()
                                                        select new BucketSkuAreaModel
                                                        {
                                                            AreaId = area.AreaId,
                                                            ShortName = area.ShortName,
                                                            BuildingId = area.BuildingId,
                                                            Description = area.Description,
                                                            InventoryPieces = subitem == null || subitem.InventoryPieces == 0 ? (int?)null : subitem.InventoryPieces,
                                                            QuantityInSmallestCarton = subitem == null ? (int?)null : subitem.PiecesInSmallestCarton
                                                        }).ToArray(),
                                     Activities = sku.Activities.ToArray()
                                 })
                                 .OrderBy(p => p.PercentCurrentPieces)
                                 .ThenBy(p => p.Style)
                                 .ThenBy(p => p.Color)
                                 .ThenBy(p => p.Dimension)
                                 .ThenBy(p => p.SkuSize)
                                 .ToArray(),
                AllAreas = allAreas,
                BucketId = bucketId,
                //StateFilter = stateFilter,
                //ActivityFilter = activityFilter
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
            return RedirectToAction(this.Actions.Wave(bucketId));
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
            var pickslips = _service.Value.GetBucketPickslip(bucketId);

            var model = new WavePickslipsViewModel
                {
                    Bucket = new BucketModel(bucket),
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
            return PartialView(this.Views._wavePickslipsPartial, model);
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
                return RedirectToAction(this.Actions.Wave(bucketId));
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
            return RedirectToAction(this.Actions.Wave(bucketId));
        }
        #endregion

        #region Wave Editor

        [Route("wave-editor")]
        public virtual ActionResult WaveEditor(int bucketId)
        {
            var bucket = _service.Value.GetBucket(bucketId);
            if (bucket == null)
            {
                // Unreasonable bucket id
                ModelState.AddModelError("", string.Format("Unreasonable Pick Wave {0}", bucketId));
                return RedirectToAction(MVC_PickWaves.PickWaves.Home.Index());
            }
            if (!bucket.IsFrozen)
            {
                // bucket is not freeze,freeze it before attempting to edit it.
                ModelState.AddModelError("", "Please freeze the pick wave before attempting to edit it");
                return RedirectToAction(Actions.Wave(bucketId));
            }

            var bucketAreas = _service.Value.GetBucketAreas(bucketId);
            var model = new WaveEditorViewModel(bucket)
            {
                PullAreaList = (from area in bucketAreas
                                where area.AreaType == BucketActivityType.Pulling
                                orderby area.CountSku descending
                                select new SelectListItem
                                {
                                    Text = string.Format("{0}: {1} ({2:N0}% SKUs available)", area.ShortName ?? area.AreaId, area.Description, area.CountOrderedSku == 0 ? 0 : area.CountSku * 100 / area.CountOrderedSku),
                                    Value = area.CountSku > 0 ? area.AreaId : "",
                                    Selected = area.AreaId == bucket.Activities[BucketActivityType.Pulling].Area.AreaId
                                }).ToList(),
                PitchAreaList = (from area in bucketAreas
                                 where area.AreaType == BucketActivityType.Pitching
                                 orderby area.CountSku descending
                                 select new SelectListItem
                                 {
                                     Text = string.Format("{0}: {1} ({2:N0}% SKUs assigned.)", area.ShortName ?? area.AreaId, area.Description, area.CountOrderedSku == 0 ? 0 : area.CountSku * 100 / area.CountOrderedSku),
                                     Value = area.CountSku > 0 ? area.AreaId : "",
                                     Selected = area.AreaId == bucket.Activities[BucketActivityType.Pitching].Area.AreaId
                                 }).ToList(),
                CustomerName = _service.Value.GetCustomerName(bucket.MaxCustomerId)
            };

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
                return RedirectToAction(this.Actions.WaveEditor(model.BucketId));
            }
            //var count = model.Bucket.Activities.Count(p => !string.IsNullOrWhiteSpace(p.AreaId));
            //if (count == 0 && model.UnfreezeWaveAfterSave)
            //{
            //    // Bucket have not any area for pulling and / pitching.
            //    ModelState.AddModelError("", "Pick wave could not be updated. Please gave at least one area for pulling and/ pitching and try again");
            //    return RedirectToAction(this.Actions.EditableWave(model.Bucket.BucketId, SuggestedNextActionType.CancelEditing));
            //}

            //var pullAreaId = model.Bucket.Activities.Single(p => p.ActivityType == BucketActivityType.Pulling).AreaId;
            //var pitchAreaId = model.Bucket.Activities.Single(p => p.ActivityType == BucketActivityType.Pitching).AreaId;
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
                return RedirectToAction(this.Actions.Wave(model.BucketId));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                //return RedirectToAction(this.Actions.EditableWave(model.BucketId, SuggestedNextActionType.NotSet));
                return RedirectToAction(this.Actions.Wave(model.BucketId));
            }

            return RedirectToAction(this.Actions.Wave(model.BucketId));
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


    }
}
