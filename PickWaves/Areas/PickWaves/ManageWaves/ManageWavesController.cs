using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository;

using DcmsMobile.PickWaves.ViewModels;

using EclipseLibrary.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
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

        private ManageWavesService _service;

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (_service == null)
            {
                _service = new ManageWavesService(this.HttpContext.Trace,
                    HttpContext.User.IsInRole(ROLE_WAVE_MANAGER) ? HttpContext.User.Identity.Name : string.Empty,
                    HttpContext.Request.UserHostName ?? HttpContext.Request.UserHostAddress
                    );
            }
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
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
        [AllowAnonymous]
        [Route("index")]
        public virtual ActionResult Index(string customerId, ProgressStage bucketState, string userName)
        {

            var buckets = _service.GetBuckets(customerId, bucketState, userName);

            var model = new IndexViewModel
            {
                CustomerId = customerId,
                BucketState = bucketState,
                UserName = userName
            };
            // Null DC Cancel dates display last
            model.Buckets = (from bucket in buckets.Select(p => new BucketModel(p))
                             orderby bucket.PriorityId descending, bucket.DcCancelDateRange.From ?? DateTime.MaxValue, bucket.PercentPiecesComplete descending
                             select bucket).ToArray();

            if (!string.IsNullOrWhiteSpace(model.CustomerId))
            {
                model.CustomerName = _service.GetCustomer(model.CustomerId) == null ? "" : _service.GetCustomer(model.CustomerId).Name;
            }
            return View(Views.Index, model);
        }

        /// <summary>
        /// Honors the passed bucketid, displayEditable and HighlightedActions
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("wave")]
        public virtual ActionResult Wave(int bucketId, SuggestedNextActionType nextAction)
        {
            var bucket = _service.GetBucket(bucketId);
            if (bucket == null)
            {
                // Unreasonable bucket id
                //ModelState.AddModelError("", string.Format("Pick Wave {0} is deleted", model.Bucket.BucketId));
                return RedirectToAction(MVC_PickWaves.PickWaves.Home.Index());
            }
            var model = new WaveViewModel
            {
                HighlightedActions = nextAction
            };
            model.Bucket = new BucketModel(bucket);

            // If Bucket is pulling bucket and value of PullingBucket is N. then Bucket Required Box Expediting
            if (!string.IsNullOrWhiteSpace(bucket.PullingBucket) && bucket.PullingBucket == "N")
            {
                model.Bucket.RequiredBoxExpediting = true;
            }
            if (!model.Bucket.IsFrozen)
            {
                model.HighlightedActions = SuggestedNextActionType.UnfreezeOthers;
            }
            return View(this.Views.Wave, model);
        }

        /// <summary>
        /// We check bucket is editable or not.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("editable")]
        [Obsolete]
        public virtual ActionResult EditableWave(int bucketId, SuggestedNextActionType suggestedNextAction)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(MVC_PickWaves.PickWaves.Home.Index());
            }
            var bucket = _service.GetBucket(bucketId);
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
                return RedirectToAction(Actions.Wave(bucketId, SuggestedNextActionType.NotSet));
            }

            var model = new WaveViewModel
            {
                HighlightedActions = suggestedNextAction,
                Bucket = new BucketModel(bucket)
            };


            var bucketAreas = _service.GetBucketAreas(model.Bucket.BucketId);
            model.BucketAreaLists = new Dictionary<BucketActivityType, IList<SelectListItem>>
                {
                    {
                        BucketActivityType.Pulling,
                        (from area in bucketAreas
                         where area.AreaType == BucketActivityType.Pulling
                         orderby area.CountSku descending
                            select new SelectListItem
                            {
                                Text = string.Format("{0}: {1} ({2:N0}% SKUs available)", area.ShortName ?? area.AreaId, area.Description, area.CountOrderedSku == 0 ? 0 : area.CountSku * 100 / area.CountOrderedSku),
                                Value = area.CountSku > 0 ? area.AreaId : "",
                                Selected = area.AreaId == bucket.Activities[BucketActivityType.Pulling].Area.AreaId
                            }).ToArray()
                      },
                      {
                            BucketActivityType.Pitching,
                            (from area in bucketAreas
                             where area.AreaType == BucketActivityType.Pitching
                             orderby area.CountSku descending
                             select new SelectListItem
                                {
                                   Text = string.Format("{0}: {1} ({2:N0}% SKUs assigned.)", area.ShortName ?? area.AreaId, area.Description, area.CountOrderedSku == 0 ? 0 : area.CountSku * 100 / area.CountOrderedSku),
                                   Value = area.CountSku > 0 ? area.AreaId : "",
                                   Selected = area.AreaId == bucket.Activities[BucketActivityType.Pitching].Area.AreaId
                                }).ToArray()
                       }
                };

            //For Pull area
            if (model.BucketAreaLists[BucketActivityType.Pulling].Any(p => string.IsNullOrWhiteSpace(p.Value)))
            {
                // We have some areas with no ordered SKUs
                model.BucketAreaLists[BucketActivityType.Pulling] = model.BucketAreaLists[BucketActivityType.Pulling].Where(p => !string.IsNullOrWhiteSpace(p.Value)).ToList();
                if (!model.BucketAreaLists[BucketActivityType.Pulling].Any())
                {
                    // We do have pull areas but none with SKUs needed
                    model.BucketAreaLists[BucketActivityType.Pulling].Add(new SelectListItem
                    {
                        Text = "(Ordered SKUs are not available in any Pull Area)",
                        Value = "",
                        Selected = true
                    });
                }
            }

            //For Pitch area
            if (model.BucketAreaLists[BucketActivityType.Pitching].Any(p => string.IsNullOrWhiteSpace(p.Value)))
            {
                // We have some areas with no ordered SKUs
                model.BucketAreaLists[BucketActivityType.Pitching] = model.BucketAreaLists[BucketActivityType.Pitching].Where(p => !string.IsNullOrWhiteSpace(p.Value)).ToList();
                if (!model.BucketAreaLists[BucketActivityType.Pitching].Any())
                {
                    // We do have pitch areas but none with SKUs needed
                    model.BucketAreaLists[BucketActivityType.Pitching].Add(new SelectListItem
                    {
                        Text = "(Ordered SKUs are not assigned in any Pitch Area)",
                        Value = "",
                        Selected = true
                    });
                }
            }

            // For edit wave
            model.DisplayEditableWave = true;
            model.BucketNameOriginal = bucket.BucketName;
            model.PriorityIdOriginal = bucket.PriorityId;
            model.PullAreaOriginal = bucket.Activities.Single(p => p.ActivityType == BucketActivityType.Pulling).Area.AreaId;
            model.PitchAreaOriginal = bucket.Activities.Single(p => p.ActivityType == BucketActivityType.Pitching).Area.AreaId;
            model.BucketCommentOriginal = bucket.BucketComment;
            model.QuickPitchOriginal = bucket.QuickPitch;
            model.PitchLimitOriginal = bucket.PitchLimit;
            

            if (!string.IsNullOrWhiteSpace(bucket.PullingBucket) && bucket.PullingBucket == "N")
            {
                // If pulling bucket
                model.Bucket.RequiredBoxExpediting = true;
                model.RequiredBoxExpeditingOriginal = true;
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
            var skuList = (from item in _service.GetBucketSkuList(bucketId)
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
                 BoxesList = (from box in _service.GetBucketBoxes(bucketId)
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
        /// Ajax call.
        /// Showing pickslip list of bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("wavepickslip")]
        public virtual ActionResult WavePickslips(int bucketId)
        {
            var bucket = _service.GetBucket(bucketId);
            var pickslips = _service.GetBucketPickslip(bucketId);

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

        /// <summary>
        /// Update passed bucket
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("editwave")]
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
            var bucket = new Bucket
            {
                BucketId = model.BucketId,
                BucketName = model.BucketName,
                PriorityId = model.PriorityId,
                BucketComment = model.BucketComment,
                QuickPitch = !string.IsNullOrEmpty(model.PitchAreaId) && model.QuickPitch
            };
            if (!string.IsNullOrEmpty(model.PitchAreaId) && model.PitchLimit != null)
            {
                bucket.PitchLimit = model.PitchLimit;
            }

            // For manage PullToDock flag. In case of pulling, PullToDock is not null.
            if (!string.IsNullOrEmpty(model.PullAreaId))
            {
                if (model.RequiredBoxExpediting)
                {
                    bucket.PullingBucket = "N";
                }
                else
                {
                    bucket.PullingBucket = "Y";
                }
            }
            // In case of pitching, PullToDock is null.
            else
            {
                bucket.PullingBucket = null;
            }

            bucket.Activities[BucketActivityType.Pulling].Area.AreaId = model.PullAreaId;
            bucket.Activities[BucketActivityType.Pitching].Area.AreaId = model.PitchAreaId;
            //var bucketOld = new Bucket
            //{
            //    BucketId = model.Bucket.BucketId,
            //    BucketName = model.BucketNameOriginal,
            //    PriorityId = model.PriorityIdOriginal,
            //    RequiredBoxExpediting = model.RequiredBoxExpeditingOriginal,
            //    BucketComment = model.BucketCommentOriginal,
            //    QuickPitch = model.QuickPitchOriginal,
            //    PitchLimit = model.PitchLimitOriginal
            //};

            //bucketOld.Activities[BucketActivityType.Pulling].Area.AreaId = model.PullAreaOriginal;
            //bucketOld.Activities[BucketActivityType.Pitching].Area.AreaId = model.PitchAreaOriginal;
            try
            {
                _service.EditWave(bucket);
                AddStatusMessage(string.Format("Pick Wave {0} updated.", model.BucketId));
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", "Pick wave could not be updated. Please review the error and try again");
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(this.Actions.EditableWave(model.BucketId, SuggestedNextActionType.NotSet));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(this.Actions.EditableWave(model.BucketId, SuggestedNextActionType.NotSet));
            }
            if (model.UnfreezeWaveAfterSave)
            {
                //  if user says unfreeze bucket after editing.
                _service.FreezeWave(model.BucketId, false);
                AddStatusMessage(string.Format("Pick wave {0} has been unfrozen.", model.BucketId));
                return RedirectToAction(this.Actions.Wave(model.BucketId, SuggestedNextActionType.UnfreezeOthers));
            }
            return RedirectToAction(this.Actions.Wave(model.BucketId, SuggestedNextActionType.UnfreezeMe));
        }
        /// <summary>
        /// Returns flags corresponding to changed values. Compares original values with current values.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Obsolete]
        private EditBucketFlags GetEditFlags(WaveViewModel model)
        {
            var flags = EditBucketFlags.None;
            if (model.BucketNameOriginal != model.Bucket.BucketName)
            {
                flags |= EditBucketFlags.BucketName;
            }
            if (model.PriorityIdOriginal != model.Bucket.PriorityId)
            {
                flags |= EditBucketFlags.Priority;
            }
            if (model.PullAreaOriginal != model.Bucket.Activities.Single(p => p.ActivityType == BucketActivityType.Pulling).AreaId)
            {
                flags |= EditBucketFlags.PullArea;
            }
            if (model.PitchAreaOriginal != model.Bucket.Activities.Single(p => p.ActivityType == BucketActivityType.Pitching).AreaId)
            {
                flags |= EditBucketFlags.PitchArea;
            }
            if (model.BucketCommentOriginal != model.Bucket.BucketComment)
            {
                flags |= EditBucketFlags.Remarks;
            }
            if (model.QuickPitchOriginal != model.Bucket.QuickPitch && !string.IsNullOrWhiteSpace(model.Bucket.Activities.Single(p => p.ActivityType == BucketActivityType.Pitching).AreaId))
            {
                flags |= EditBucketFlags.QuickPitch;
            }
            if (model.PitchLimitOriginal != model.Bucket.PitchLimit && !string.IsNullOrWhiteSpace(model.Bucket.Activities.Single(p => p.ActivityType == BucketActivityType.Pitching).AreaId))
            {
                flags |= EditBucketFlags.PitchLimit;
            }
            flags |= EditBucketFlags.PullType;
            return flags;
        }

        /// <summary>
        /// Increase priority.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns>Returns the value of the updated priority</returns>
        [HttpPost]
        [Route("priority")]
        public virtual ActionResult IncrementPriority(int bucketId)
        {
            var priority = _service.IncrementPriority(bucketId);
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
            var priority = _service.DecrementPriority(bucketId);
            return Json(priority);
        }

        /// <summary>
        /// This method is used to freeze or unfreeze any bucket.
        /// </summary>
        /// <param name="bucketId"> </param>
        /// <param name="freeze"> </param>
        /// <param name="displayEditable">Whether we should redirect to ediateble wave or read only wave</param>
        /// <returns></returns>
        [HttpPost]
        [Route("freeze")]
        public virtual ActionResult FreezeBucket(int bucketId, bool freeze, bool displayEditable = false)
        {
            try
            {
                _service.FreezeWave(bucketId, freeze);
                if (freeze)
                {
                    // user has frozen a bucket.
                    AddStatusMessage(string.Format("Pick wave {0} has been frozen.", bucketId));
                    if (displayEditable)
                    {
                        return RedirectToAction(this.Actions.EditableWave(bucketId, SuggestedNextActionType.CancelEditing
                            | SuggestedNextActionType.FreezeOthers));
                    }
                    else
                    {
                        return RedirectToAction(this.Actions.Wave(bucketId, SuggestedNextActionType.EditMe | SuggestedNextActionType.FreezeOthers));
                    }
                }
                else
                {
                    // User has unfrozen a bucket.
                    AddStatusMessage(string.Format("Pick wave {0} has been unfrozen.", bucketId));
                    return RedirectToAction(this.Actions.Wave(bucketId, SuggestedNextActionType.UnfreezeOthers));
                }
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(this.Actions.EditableWave(bucketId, SuggestedNextActionType.CancelEditing));
            }
        }

        /// <summary>
        /// Remove passed pickslip from bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="pickslipId"></param>
        /// <returns></returns>
        [Route("removepsformbucket")]
        public virtual ActionResult RemovePickslipFromBucket(int bucketId, long pickslipId)
        {
            try
            {
                _service.RemovePickslipFromBucket(pickslipId, bucketId);
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

        [Route("wave-editor")]
        public virtual ActionResult WaveEditor(int bucketId)
        {
            var bucket = _service.GetBucket(bucketId);
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
                return RedirectToAction(Actions.Wave(bucketId, SuggestedNextActionType.NotSet));
            }

            var bucketAreas = _service.GetBucketAreas(bucketId);
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
                                 }).ToList()
            };
            



            //model.BucketAreaLists = new Dictionary<BucketActivityType, IList<SelectListItem>>
            //    {
            //        {
            //            BucketActivityType.Pulling,
            //            (from area in bucketAreas
            //             where area.AreaType == BucketActivityType.Pulling
            //             orderby area.CountSku descending
            //                select new SelectListItem
            //                {
            //                    Text = string.Format("{0}: {1} ({2:N0}% SKUs available)", area.ShortName ?? area.AreaId, area.Description, area.CountOrderedSku == 0 ? 0 : area.CountSku * 100 / area.CountOrderedSku),
            //                    Value = area.CountSku > 0 ? area.AreaId : "",
            //                    Selected = area.AreaId == bucket.Activities[BucketActivityType.Pulling].Area.AreaId
            //                }).ToArray()
            //          },
            //          {
            //                BucketActivityType.Pitching,
            //                (from area in bucketAreas
            //                 where area.AreaType == BucketActivityType.Pitching
            //                 orderby area.CountSku descending
            //                 select new SelectListItem
            //                    {
            //                       Text = string.Format("{0}: {1} ({2:N0}% SKUs assigned.)", area.ShortName ?? area.AreaId, area.Description, area.CountOrderedSku == 0 ? 0 : area.CountSku * 100 / area.CountOrderedSku),
            //                       Value = area.CountSku > 0 ? area.AreaId : "",
            //                       Selected = area.AreaId == bucket.Activities[BucketActivityType.Pitching].Area.AreaId
            //                    }).ToArray()
            //           }
            //    };

            //For Pull area
            //if (model.BucketAreaLists[BucketActivityType.Pulling].Any(p => string.IsNullOrWhiteSpace(p.Value)))
            //{
            //    // We have some areas with no ordered SKUs
            //    model.BucketAreaLists[BucketActivityType.Pulling] = model.BucketAreaLists[BucketActivityType.Pulling].Where(p => !string.IsNullOrWhiteSpace(p.Value)).ToList();
            //    if (!model.BucketAreaLists[BucketActivityType.Pulling].Any())
            //    {
            //        // We do have pull areas but none with SKUs needed
            //        model.BucketAreaLists[BucketActivityType.Pulling].Add(new SelectListItem
            //        {
            //            Text = "(Ordered SKUs are not available in any Pull Area)",
            //            Value = "",
            //            Selected = true
            //        });
            //    }
            //}

            //For Pitch area
            //if (model.BucketAreaLists[BucketActivityType.Pitching].Any(p => string.IsNullOrWhiteSpace(p.Value)))
            //{
            //    // We have some areas with no ordered SKUs
            //    model.BucketAreaLists[BucketActivityType.Pitching] = model.BucketAreaLists[BucketActivityType.Pitching].Where(p => !string.IsNullOrWhiteSpace(p.Value)).ToList();
            //    if (!model.BucketAreaLists[BucketActivityType.Pitching].Any())
            //    {
            //        // We do have pitch areas but none with SKUs needed
            //        model.BucketAreaLists[BucketActivityType.Pitching].Add(new SelectListItem
            //        {
            //            Text = "(Ordered SKUs are not assigned in any Pitch Area)",
            //            Value = "",
            //            Selected = true
            //        });
            //    }
            //}

            // For edit wave
            //model.DisplayEditableWave = true;
            //model.BucketNameOriginal = bucket.BucketName;
            //model.PriorityIdOriginal = bucket.PriorityId;
            //model.PullAreaOriginal = bucket.Activities.Single(p => p.ActivityType == BucketActivityType.Pulling).Area.AreaId;
            //model.PitchAreaOriginal = bucket.Activities.Single(p => p.ActivityType == BucketActivityType.Pitching).Area.AreaId;
            //model.BucketCommentOriginal = bucket.BucketComment;
            //model.QuickPitchOriginal = bucket.QuickPitch;
            //model.PitchLimitOriginal = bucket.PitchLimit;

            //if (!string.IsNullOrWhiteSpace(bucket.PullingBucket) && bucket.PullingBucket == "N")
            //{
            //    // If pulling bucket
            //    model.Bucket.RequiredBoxExpediting = true;
            //    model.RequiredBoxExpeditingOriginal = true;
            //}

            return View(Views.WaveEditor,model);
        }
    }
}
