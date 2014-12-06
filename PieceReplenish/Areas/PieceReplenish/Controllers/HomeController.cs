
using DcmsMobile.PieceReplenish.Repository.Home;
using DcmsMobile.PieceReplenish.ViewModels;
using EclipseLibrary.Mvc.Controllers;
using EclipseLibrary.Oracle.Helpers;
using System;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Web.Mvc;
using System.Web.WebPages;

namespace DcmsMobile.PieceReplenish.Areas.PieceReplenish.Controllers
{
    [RouteArea("PieceReplenish")]
    [RoutePrefix(HomeController.NameConst)]
    public partial class HomeController : EclipseController
    {
        #region Intialization

        public HomeController()
        {

        }

        private HomeService _service;

        public HomeService Service
        {

            get { return _service; }

            set { _service = value; }
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

        private const string ROLE_PUL = "SRC_PULLING";

        private const string ROLE_MANAGE_REPLENISHMENT = "DCMS8_REPLENISH";

        /// <summary>
        /// Displays the main menu
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Index()
        {
            return View(Views.Index);
        }

        /// <summary>
        /// Toggles Mobile emulation
        /// </summary>
        /// <param name="emulateMobile"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public virtual ActionResult ToggleEmulation()
        {
            if (HttpContext.GetOverriddenBrowser().IsMobileDevice)
            {
                HttpContext.ClearOverriddenBrowser();
            }
            else
            {
                HttpContext.SetOverriddenBrowser(BrowserOverride.Mobile);
            }

            return RedirectToAction(Actions.Index());
        }

        /// <summary>
        /// Displays a list of buildings where there is work
        /// TC5: Click Replenishment Pulling link on main menu
        /// </summary>
        /// <returns></returns>
        [Route(HomeController.ActionNameConstants.Building, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_PieceReplenish)]
        public virtual ActionResult Building(bool forceQuery = false)
        {
            if (forceQuery)
            {
                // We force the query to execute
                // TC1: Click Recalculate Carton Requirements link on the Building View
                try
                {
                    _service.RefreshPullableCartons();
                    this.AddStatusMessage("Refresh has been initiated");
                }
                catch (DbException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                // We redirect because refresh of this page should not pass true again
                return RedirectToAction(this.Actions.Building());
            }
            var model = new BuildingViewModel
            {
                BuildingAreaChoiceList = from choice in _service.GetAreaList()
                                         select new BuildingAreaModel
                                         {
                                             BuildingId = choice.BuildingId,
                                             CartonAreaId = choice.CartonAreaId,
                                             CartonCount = choice.PullableCartonCount,
                                             PickAreaId = choice.AreaId,
                                             RestockAreaId = choice.RestockAreaId,
                                             ShortName = choice.ShortName
                                         }
            };

            model.GenerateStats += new EventHandler<BuildingStatsEventArgs>(bvm_OnGenerateStats);
            return View(Views.Building, model);
        }

        /// <summary>
        /// Called from the mobile menu. Requires authorization so that login is asked for before pulling begins.
        /// </summary>
        /// <param name="forceQuery"></param>
        /// <returns></returns>
        [AuthorizeEx("Piece Replenishment requires Role {0}", Roles = ROLE_PUL, Purpose = "Enables the pullers to replenish the picking areas")]
        public virtual ActionResult BuildingMobile()
        {
            return Building();
        }
        /// <summary>
        /// This will be called only if the app is running on the desktop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bvm_OnGenerateStats(object sender, BuildingStatsEventArgs e)
        {
            // TC2: See the Building page on desktop to ensure that the puller activities displayed are correct.
            var activities = _service.GetCartonsBeingPulled(null);
            e.PullerActivities = (from activity in activities
                                  select new PullerActivityModel
                                  {
                                      BuildingId = activity.BuildingId,
                                      PalletId = activity.PalletId,
                                      CartonCount = activity.CartonCount,
                                      IsUsingReplenishmentModule = activity.IsUsingReplenishmentModule,
                                      ListSkuId = activity.ListSkuId,
                                      MinAssignDate = activity.MinAssignDate,
                                      PullerName = activity.PullerName,
                                      Styles = activity.Styles,
                                      RestockAisleId = activity.RestockAisleId
                                  }).ToArray();
            try
            {
                var info = _service.GetRefreshInfo();
                if (info != null)
                {
                    e.IsRefreshingNow = info.IsRefreshingNow;
                }
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", "Unable to get information about last job performed to update the Replenishment requirements.<br />" + ex.Message);
            }
        }

        /// <summary>
        /// Displays the pallet view. Enter building in building view
        /// </summary>
        /// <param name="context"> </param>
        /// <param name="forceQuery"></param>
        /// <returns></returns>
        [AuthorizeEx("Piece Replenishment requires Role {0}", Roles = ROLE_PUL, Purpose = "Enables the pullers to replenish the picking areas")]
        public virtual ActionResult Pallet(string context, bool forceQuery = false)
        {
            if (forceQuery)
            {
                // We force the query to execute
                // TC3: Click initiate update now on Pallet desktop page to force the query to execute
                try
                {
                    _service.RefreshPullableCartons();
                    this.AddStatusMessage("Refresh has been initiated");
                }
                catch (DbException ex)
                {
                    ModelState.AddModelError("", "Some problem occurred, Could not update Replenishment requirements.<br/>" + ex.Message);
                }
                // We redirect because refresh of this page should not pass true again
                return RedirectToAction(this.Actions.Pallet(context));
            }
            var pvm = new PalletViewModel
            {
                Context = new ContextModel
                {
                    Serialized = context,
                }
            };

            pvm.GenerateStats += new EventHandler<GenerateStatsEventArgs>(pvm_OnGenerateStats);
            return View(Views.Pallet, pvm);
        }

        void pvm_OnGenerateStats(object sender, GenerateStatsEventArgs e)
        {
            // TC4: View SKU list on desktop page
            var pvm = (PalletViewModel)sender;
            var aisleSkuList = _service.GetSkusToPull(pvm.Context.BuildingId, pvm.Context.PickAreaId, pvm.Context.CartonAreaId, pvm.Context.RestockAreaId);

            e.AisleReplenishmentStats = (from aislesku in aisleSkuList
                                         group aislesku by aislesku.RestockAisleId into g
                                         let maxWavePriority = g.Max(p => p.WavePriority)
                                         orderby g.Max(p => p.SkuReplenishmentPriority) descending,
                                            g.Count(p => p.SkuReplenishmentPriority.HasValue) descending,
                                            maxWavePriority descending,
                                            g.Where(p => p.WavePriority == maxWavePriority).Max(p => p.WaveCount) descending
                                         select new AisleReplenishmentModel
                                         {
                                             RestockAisleId = g.Key,
                                             SkuList = (from item in g
                                                        orderby item.SkuReplenishmentPriority descending, item.WavePriority descending, item.WaveCount descending,
                                                        item.Style, item.Color, item.Dimension, item.SkuSize
                                                        select new SkuModel
                                                        {
                                                            VwhId = item.VwhId,
                                                            Style = item.Style,
                                                            Color = item.Color,
                                                            Dimension = item.Dimension,
                                                            SkuSize = item.SkuSize,
                                                            UpcCode = item.UpcCode,
                                                            SkuId = item.SkuId,
                                                            AisleCapacity = item.Capacity,
                                                            PiecesInAisle = item.PiecesAtPickLocations,
                                                            PiecesInRestock = item.PiecesAwaitingRestock,
                                                            PiecesInPullableCarton = item.PiecesInPullableCarton,
                                                            PiecesToPick = item.PiecesToPick,
                                                            SkuReplenishmentPriority = item.SkuReplenishmentPriority,
                                                            WavePriority = item.WavePriority,
                                                            WaveCount = item.WaveCount,
                                                            CartonsToPull = item.CartonsToPull,
                                                            CartonsInRestock = item.CartonsInRestock,
                                                            PercentInAisle = (int)Math.Round((double)(item.PiecesAtPickLocations ?? 0) * 100 / (double)item.Capacity),
                                                            PercentInRestock = (int)Math.Round((double)(item.PiecesAwaitingRestock ?? 0) * 100 / (double)item.Capacity),
                                                            PercentToPull = (int)Math.Round((double)(item.PiecesInPullableCarton) * 100 / (double)item.Capacity)
                                                        }).ToArray(),
                                             TotalPiecesToPull = g.Sum(p => p.PiecesInPullableCarton),
                                             PiecesInRestock = g.Sum(p => p.PiecesAwaitingRestock),
                                             CartonsToPull = g.Sum(p => p.CartonsToPull),
                                             CartonsInRestock = g.Sum(p => p.CartonsInRestock ?? 0),
                                             PiecesInAisle = g.Sum(p => p.PiecesAtPickLocations),
                                             Capacity = g.Sum(p => p.Capacity),
                                         }).ToArray();

            var pullers = _service.GetCartonsBeingPulled(pvm.Context.BuildingId);
            var cartons = _service.GetProposedCartonSuggestions(pvm.Context.BuildingId, pvm.Context.PickAreaId, pvm.Context.CartonAreaId, pvm.Context.RestockAreaId, 10);

            foreach (var aisle in e.AisleReplenishmentStats)
            {
                var aislePullers = pullers.Where(p => p.RestockAisleId == aisle.RestockAisleId);
                aisle.Pullers = string.Join(", ", aislePullers.Select(p => p.PullerName).Distinct());
                var aisleSkuIdBeingPulled = aislePullers.SelectMany(p => p.ListSkuId);
                foreach (var sku in aisle.SkuList.Where(p => aisleSkuIdBeingPulled.Contains(p.SkuId)))
                {
                    sku.BeingPulled = true;
                }
                var nextSkuList = cartons.Where(p => p.RestockAisleId == aisle.RestockAisleId).Select(p => p.SkuInCarton.SkuId);
                foreach (var sku in aisle.SkuList.Where(p => nextSkuList.Contains(p.SkuId) && !aisleSkuIdBeingPulled.Contains(p.SkuId)))
                {
                    sku.WillGetPulledNext = true;
                }
                aisle.PercentInAisle = (int)Math.Round((double)(aisle.PiecesInAisle ?? 0) * 100 / (double)aisle.Capacity);
                aisle.PercentInRestock = (int)Math.Round((double)(aisle.PiecesInRestock ?? 0) * 100 / (double)aisle.Capacity);
                aisle.PercentToPull = (int)Math.Round((double)(aisle.TotalPiecesToPull) * 100 / (double)aisle.Capacity);
            }
            try
            {
                var info = _service.GetRefreshInfo();
                if (info != null)
                {
                    e.QueryTime = info.LastRefreshedTime;
                    e.NextRunDate = info.NextRunDate;
                    e.IsRefreshingNow = info.IsRefreshingNow;
                }
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", "Unable to get information about last job performed to update the Replenishment requirements.<br />" + ex.Message);
            }
        }

        /// <summary>
        /// Accepts the scanned pallet on Pallet View, and returns the Carton view 
        /// </summary>
        /// <returns></returns>
        [AuthorizeEx("Piece Replenishment requires Role {0}", Roles = ROLE_PUL, Purpose = "Enables the pullers to replenish the picking areas")]
        public virtual ActionResult AcceptPallet(PalletViewModel pvm)
        {
            if (string.IsNullOrEmpty(pvm.PalletId))
            {
                //TC6: Enter pressed back to change the bldg/area
                return RedirectToAction(Actions.Building());
            }
            if (!ModelState.IsValid)
            {
                // TC7: When will the model state be invalid?
                pvm.GenerateStats += new EventHandler<GenerateStatsEventArgs>(pvm_OnGenerateStats);
                return View(Views.Pallet, pvm);
            }
            return RedirectToAction(this.Actions.Carton(pvm.Context.Serialized, pvm.PalletId));
        }

        /// <summary>
        /// Prompts for carton. Get here by entering pallet on the pallet page.
        /// </summary>
        /// <returns></returns>
        [AuthorizeEx("Piece Replenishment requires Role {0}", Roles = ROLE_PUL, Purpose = "Enables the pullers to replenish the picking areas")]
        public virtual ActionResult Carton(string serializedContext, string palletId, char sound = '\0')
        {

            var ctx = new ContextModel
            {
                Serialized = serializedContext
            };
            int oldSuggestionCount;
            palletId = palletId.Trim().ToUpper();
            var cartons = _service.GetCartonSuggestions(ctx.BuildingId, ctx.PickAreaId, ctx.RestockAreaId, palletId, 5, out oldSuggestionCount);
            if (!cartons.Any())
            {
                // TC8: No cartons available for replenishing
                AddStatusMessage(string.Format("Nothing to pull for [{0}] {1} -> {2} -> {3}", ctx.BuildingId, ctx.CartonAreaId, ctx.RestockAreaId, ctx.ShortName));
                return Pallet(serializedContext);
            }

            var pallet = _service.GetPallet(palletId);

            //If pallet already contains cartons, making sure that the puller is continuing with partially pulled pallet or any other random pallet.
            //If it is a random pallet, then aware the puller for case of aisle mixing.
            if (pallet.CartonCount > 0 && oldSuggestionCount == 0)
            {
                AddStatusMessage(string.Format("Pallet {0} already contains cartons {1}, Make sure that you are not mixing cartons of different aisles.", pallet.PalletId, pallet.CartonCount));
            }
            var cartonList = from carton in cartons
                             select new CartonModel
                                        {
                                            CartonId = carton.CartonId,
                                            LocationId = carton.LocationId,
                                            SkuInCarton = new SkuModel
                                                          {
                                                              SkuId = carton.SkuInCarton.SkuId,
                                                              Color = carton.SkuInCarton.Color,
                                                              Style = carton.SkuInCarton.Style,
                                                              Dimension = carton.SkuInCarton.Dimension,
                                                              SkuSize = carton.SkuInCarton.SkuSize,
                                                              UpcCode = carton.SkuInCarton.UpcCode
                                                          },
                                            SkuReplenishmentPriority = carton.SkuPriority
                                        };

            var cvm = new CartonViewModel
            {
                PalletId = palletId,
                CartonList = cartonList,
                Context = new ContextModel
                {
                    Serialized = serializedContext,
                },
                RestockAisleId = cartons.First().RestockAisleId,
                CountCartonsOnPallet = pallet.CartonCount,
                PriceSeasonCode = pallet.PriceSeasonCode,
                Sound = sound,
                IsPuller = AuthorizeExAttribute.IsSuperUser(HttpContext) || HttpContext.User.IsInRole(ROLE_PUL)
            };

            return View(Views.Carton, cvm);
        }

        /// <summary>
        /// Pulls the carton entered by user
        /// </summary>
        /// <param name="cvm"></param>
        /// <returns></returns>
        /// <remarks>
        /// Must post CartonId, Context, RestockAreaId, PalletId, RestockAisleId
        /// </remarks>
        [HttpPost]
        [AuthorizeEx("Piece Replenishment requires Role {0}", Roles = ROLE_PUL, Purpose = "Enables the pullers to replenish the picking areas")]
        public virtual ActionResult PullCarton(CartonViewModel cvm)
        {
            if (string.IsNullOrWhiteSpace(cvm.CartonId))
            {
                // TC9: Go back. Enter blank carton, to get prompted for pallet again
                return RedirectToAction(this.Actions.Pallet(cvm.Context.Serialized));
            }
            cvm.CartonId = cvm.CartonId.Trim().ToUpper();
            if (!ModelState.IsValid)
            {
                // TC10: When will this happen?
                return View(this.Views.Carton, cvm);
            }
            try
            {
                int countSuggestions;
                var ispulled = _service.TryPullCarton(cvm.CartonId, cvm.PalletId, cvm.Context.RestockAreaId, cvm.RestockAisleId, out countSuggestions);

                if (ispulled)
                {
                    AddStatusMessage(string.Format("Carton {0} is now on Pallet {1}", cvm.CartonId, cvm.PalletId));
                    cvm.Sound = 'S';
                }
                else
                {
                    //DB,Ravneet: Not too happy writing this but works for now. Sorry. 
                    ModelState.AddModelError("", string.Format("Carton {0} is either invalid or can not pull for aisle {1}", cvm.CartonId, cvm.RestockAisleId));
                    cvm.Sound = 'E';
                }


                if (countSuggestions == 0)
                {
                    // TC11: You have just pulled the last carton of the restock aisle.
                    AddStatusMessage(string.Format("Pulling of Pallet {0} is complete.", cvm.PalletId));
                    return RedirectToAction(this.Actions.Pallet(cvm.Context.Serialized));
                }
            }
            catch (OracleDataStoreException ex)
            {
                ModelState.AddModelError("", ex.Message);
                cvm.Sound = 'E';
                //return Carton(cvm.Context.Serialized, cvm.PalletId);
            }
            return RedirectToAction(Actions.Carton(cvm.Context.Serialized, cvm.PalletId, cvm.Sound));
        }

        /// <summary>
        /// Get here by clicking on the sequence number f the carton in the list of suggestions on carton page
        /// </summary>
        /// <param name="serializedContext"></param>
        /// <param name="palletId"></param>
        /// <param name="sourceLocationId"></param>
        /// <param name="cartonId"></param>
        /// <returns></returns>
        [AuthorizeEx("Piece Replenishment requires Role {0}", Roles = ROLE_PUL, Purpose = "Enables the pullers to replenish the picking areas")]
        public virtual ActionResult SkipCarton(string serializedContext, string palletId, string sourceLocationId, string cartonId)
        {
            var pallet = _service.GetPallet(palletId);
            var model = new SkipCartonViewModel()
                            {
                                Context = new ContextModel
                                {
                                    Serialized = serializedContext,
                                },
                                PalletId = palletId,
                                CountCartonsOnPallet = pallet.CartonCount,
                                SourceLocationId = sourceLocationId,
                                CartonId = cartonId,
                                Sound = 'W'
                            };
            return View(Views.SkipCarton, model);
        }

        /// <summary>
        /// If confirmation received, places the posted carton in suspense. If confirmation scan is empty, it goes back. If it does not match then error and same view
        /// displayed again.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// Post Data:  BuildingId, PickAreaId, CartonAreaId, RestockAreaId, RestockAisleId, PalletId, CartonId, ConfirmLocationId, SourceLocationId
        /// </remarks>
        [HttpPost]
        [AuthorizeEx("Piece Replenishment requires Role {0}", Roles = ROLE_PUL, Purpose = "Enables the pullers to replenish the picking areas")]
        public virtual ActionResult MoveCartonToSuspense(SkipCartonViewModel model)
        {
            try
            {
                switch (model.Choice)
                {
                    case "":
                    case null:
                        break;

                    case "1":
                        // Send to suspense
                        var isMarked = _service.MarkCartonInSuspense(model.CartonId, model.SourceLocationId);
                        if (isMarked)
                        {
                            AddStatusMessage(string.Format("Carton {0} marked in suspense.", model.CartonId));
                        }
                        else
                        {
                            AddStatusMessage(string.Format("Unable to mark Carton {0} in suspense, May be carton already pulled.", model.CartonId));
                        }
                        break;

                    case "2":
                        //remove carton from puller suggestion list
                        _service.RemoveCartonSuggestion(model.CartonId);
                        AddStatusMessage(string.Format("Carton {0} removed from your sugg. list.", model.CartonId));
                        break;
                }
                return RedirectToAction(Actions.Carton(model.Context.Serialized, model.PalletId));
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(Views.SkipCarton, model);
            }

        }

        /// <summary>
        /// Sets the priority of SKU
        /// </summary>
        /// <param name="buildingId">Locations belong to which building, on where passed <paramref name="skuId"/> is assigned.</param>
        /// <param name="pickAreaId">Locations belong to which area, on where passed <paramref name="skuId"/> is assigned.</param>
        /// <param name="skuId">Sku_id of passed SKU</param>
        /// <param name="setHighPriorityFlag">
        /// If true -> upgrade the SKU priority to Higher
        /// If false -> degrade the SKU priority to normal
        /// </param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Piece Replenishment requires Role {0}", Roles = ROLE_MANAGE_REPLENISHMENT, Purpose = "To manage piece replenishment and to set the SKU priority")]
        public virtual ActionResult SetPriority(string buildingId, string pickAreaId, int skuId, bool setHighPriorityFlag)
        {
            Response.StatusCode = 202;
            if (setHighPriorityFlag)
            {
                // TC11: Increasing priority
                var expiryTime = _service.IncreaseSkuPriority(buildingId, pickAreaId, skuId);
                if (expiryTime == null)
                {
                    return Content("SKU is not assigned at any location.");
                }
                return Content(string.Format("Priority of SKU is set to high until {0:t}", expiryTime));
            }
            // TC12: Decreasing priority
            var b = _service.DecreaseSkuPriority(buildingId, pickAreaId, skuId);
            if (b)
            {
                return Content("Priority of SKU has been set to normal");
            }
            return Content("SKU was not marked as high priority");
        }

        [AuthorizeEx("Piece Replenishment requires Role {0}", Roles = ROLE_MANAGE_REPLENISHMENT, Purpose = "To manage piece replenishment and to set the SKU priority")]
        public virtual ActionResult DiscardPalletSuggestion(string pullerName, string palletId)
        {
            _service.DiscardPalletSuggestion(pullerName, palletId);
            return RedirectToAction(Actions.Building());
        }

        /// <summary>
        /// Overriding the OnAuthorization() to handle the issue with logged in user to execute those action method which can be called as anonymous user.
        /// Now service will be called with username for those action method which having AuthorizeExAttribute, 
        /// and which have not this attribute those will be called with super user.
        /// </summary>
        /// <param name="filterContext"></param>
        //protected override void OnAuthorization(AuthorizationContext filterContext)
        //{
        //    var connectString = ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString;
        //    var clientInfo = string.IsNullOrEmpty(this.HttpContext.Request.UserHostName) ? this.HttpContext.Request.UserHostAddress :
        //        this.HttpContext.Request.UserHostName;

        //    if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(AuthorizeExAttribute), true).Any())
        //    {
        //        // create service with user
        //        var userName = this.HttpContext.SkipAuthorization ? string.Empty : this.HttpContext.User.Identity.Name;
        //        _service = new HomeService(this.HttpContext.Trace, connectString, userName, clientInfo);
        //    }
        //    else
        //    {
        //        // create service without user
        //        _service = new HomeService(this.HttpContext.Trace, connectString, string.Empty, clientInfo);
        //    }
        //    base.OnAuthorization(filterContext);
        //}

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            var connectString = ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString;
            var clientInfo = string.IsNullOrEmpty(this.HttpContext.Request.UserHostName) ? this.HttpContext.Request.UserHostAddress : this.HttpContext.Request.UserHostName;
            // create service with user
            var userName = this.HttpContext.User.Identity.Name;
            _service = new HomeService(this.HttpContext.Trace, connectString, userName, clientInfo);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var vr = filterContext.Result as ViewResult;
            if (vr != null)
            {
                var model = vr.Model as ViewModelBase;
                if (model != null)
                {
                    model.IsEditable = AuthorizeExAttribute.IsSuperUser(HttpContext) || HttpContext.User.IsInRole(ROLE_MANAGE_REPLENISHMENT);
                    model.EditableRoleName = ROLE_MANAGE_REPLENISHMENT;
                }
            }
            base.OnActionExecuted(filterContext);
        }

        /// <summary>
        /// If we get an unknown action while posting, it means that the user has just been redirected from a login page. The return URL
        /// will not accept the redirect since it is POST only. So we redirect to the Box Manager Home Page.
        /// </summary>
        /// <param name="actionName">actionName</param>
        /// <example>
        ///  ~/DcmsMobile2011/BoxManager/Home/HandleScan?UiType=ScanToPallet
        /// </example>
        protected override void HandleUnknownAction(string actionName)
        {
            ActionResult result = null;
            // Is this a valid action ?
            var methods = this.GetType().GetMethods().Where(p => p.Name == actionName).ToArray();
            if (methods.Length == 0)
            {
                // This action no longer exists. Does the user have a book mark which is now broken?
                AddStatusMessage("The page you requested was not found. You have been redirected to the main page.");
                result = RedirectToActionPermanent(MVC_PieceReplenish.PieceReplenish.Home.Index());
            }
            else
            {

                if (this.HttpContext.Request.HttpMethod == "GET")
                {
                    var attrPost = methods.SelectMany(p => p.GetCustomAttributes(typeof(HttpPostAttribute), true)).FirstOrDefault();
                    if (attrPost != null)
                    {
                        // GET request for an action which requires POST. Assume that the user has been redirected from the login screen
                        AddStatusMessage("Please start again.");
                        result = RedirectToAction(this.Actions.Building());
                    }
                }
            }
            if (result == null)
            {
                // We really don't know what to do. Let base class handle it
                base.HandleUnknownAction(actionName);
            }
            else
            {
                result.ExecuteResult(this.ControllerContext);
            }
        }
    }
}



/*
    $Id: HomeController.cs 17725 2012-07-26 08:18:57Z bkumar $
    $Revision: 17725 $
    $URL: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/Areas/PieceReplenish/Controllers/HomeController.cs $
    $Header: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/Areas/PieceReplenish/Controllers/HomeController.cs 17725 2012-07-26 08:18:57Z bkumar $
    $Author: bkumar $
    $Date: 2012-07-26 13:48:57 +0530 (Thu, 26 Jul 2012) $
*/
