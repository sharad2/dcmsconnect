using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using DcmsMobile.PieceReplenish.Repository.Diagnostic;
using DcmsMobile.PieceReplenish.ViewModels;
using DcmsMobile.PieceReplenish.ViewModels.Diagnostic;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.PieceReplenish.Areas.PieceReplenish.Controllers
{
    public partial class DiagnosticController : EclipseController
    {

        #region Intialization

        public DiagnosticController()
        {

        }

        private DiagnosticService _service;

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
        /// Action method for such link who only passes the context to be at Index page of SKU Diagnostic controller
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual ActionResult SearchSku(string context)
        {
            var model = new SearchSkuViewModel
            {
                Context = new ContextModel
                {
                    Serialized = context
                }
            };
            return View(Views.SearchSku, model);
        }

        /// <summary>
        /// Action method for those links who pass the context and SKU to be at SKU Diagnostic page 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="skuId"></param>
        /// <param name="upcCode"></param>
        /// <returns></returns>
        public virtual ActionResult SearchSkuLink(string context, int skuId, string upcCode)
        {
            var model = new SearchSkuViewModel
                {
                    Context = new ContextModel { Serialized = context },
                    SkuBarCode = upcCode,
                    SkuId = skuId
                };
            return SearchSkuRequirement(model);
        }

        /// <summary>
        /// Search the passed SKU and diagnose its requirement and availability
        /// </summary>
        /// <param name="srm"></param>
        /// <returns></returns>
        /// <remarks>
        /// Action method expected in posted model : SkuId, Context
        /// </remarks>
        public virtual ActionResult SearchSkuRequirement(SearchSkuViewModel srm)
        {
            //TC1 : If SKU is not passed, redirect to Search SKU page again.
            if (!ModelState.IsValid)
            {
                return RedirectToAction(this.Actions.SearchSku(srm.Context.Serialized));
            }
            var locReq = _service.GetSkuRequirements(srm.SkuId);
            //TC2: If any invalid SKU was passed show the proper error and redirect the Search SKU page again.
            if (!locReq.Any())
            {
                ModelState.AddModelError("", "Invalid SKU");
                return RedirectToAction(this.Actions.SearchSku(srm.Context.Serialized));
            }
            var sku = locReq.First().Sku;
            srm.Style = sku.Style;
            srm.Color = sku.Color;
            srm.Dimension = sku.Dimension;
            srm.SkuSize = sku.SkuSize;

            var groupedRequirements = from req in locReq
                                      where !string.IsNullOrWhiteSpace(req.LocationId)
                                      group req by new SearchSkuGroup(req.BuildingId, req.VwhId, req.RestockAreaId, req.ReplenishAreaId) into g
                                      select g;
            //TC3: Cross check all the shown data in the tables
            foreach (var grp in groupedRequirements)
            {
                var requirement = from req in grp
                                  select new SkuRequirementModel
                                      {
                                          LocationId = req.LocationId,
                                          BuildingId = req.BuildingId,
                                          IsFrozen = req.IsFrozen,
                                          LocationCapacity = req.LocationCapacity,
                                          LocationType = req.LocationType,
                                          PickAreaId = req.PickAreaId,
                                          ShortName = req.ShortName,
                                          PiecesAtLocation = req.PiecesAtLocation,
                                          PiecesRequiredAtLocation = req.PiecesRequiredAtLocation,
                                          ReplenishAreaId = req.ReplenishAreaId,
                                          RestockAisleId = req.RestockAisleId,
                                          RestockAreaId = req.RestockAreaId,
                                          VwhId = req.VwhId
                                      };
                srm.GroupedSkuRequirements.Add(grp.Key, requirement.ToArray());
                var allCartons = (from carton in _service.GetCartonsOfSku(srm.SkuId, grp.Key.RestockAreaId, grp.Key.ReplenishAreaId)
                                  select new DiagnosticCartonModel
                                  {
                                      CartonId = carton.CartonId,
                                      AreaId = carton.AreaId,
                                      BuildingId = carton.BuildingId,
                                      IsBestQalityCarton = carton.IsBestQalityCarton,
                                      IsCartonDamage = carton.IsCartonDamage,
                                      IsCartonInSuspense = carton.IsCartonInSuspense,
                                      IsWorkNeeded = carton.IsWorkNeeded,
                                      LocationId = carton.LocationId,
                                      QualityCode = carton.QualityCode,
                                      Quantity = carton.Quantity,
                                      VwhId = carton.VwhId
                                  }).ToArray();
                var cartonsToRestock = from ctn in allCartons
                                       where ctn.AreaId == grp.Key.RestockAreaId
                                       group ctn by new SearchSkuGroup(ctn.BuildingId, ctn.VwhId, grp.Key.RestockAreaId, grp.Key.ReplenishAreaId) into g
                                       select g;
                foreach (var group in cartonsToRestock)
                {
                    if (!srm.GroupedCartonsToRestock.ContainsKey(group.Key))
                        srm.GroupedCartonsToRestock.Add(group.Key, group.ToArray());
                }

                var cartonsToPull = from ctn in allCartons
                                    where ctn.AreaId == grp.Key.ReplenishAreaId
                                    group ctn by new SearchSkuGroup(ctn.BuildingId, ctn.VwhId, grp.Key.RestockAreaId, grp.Key.ReplenishAreaId) into g
                                    select g;

                foreach (var group in cartonsToPull)
                {
                    var cum = 0;
                    var cartons = group.ToArray();
                    foreach (var ctn in cartons.Where(p => p.CanPullCarton))
                    {
                        cum += ctn.Quantity;
                        ctn.CumPieces = cum;
                    }
                    if (!srm.GroupedCartonsToPull.ContainsKey(group.Key))
                        srm.GroupedCartonsToPull.Add(group.Key, cartons);
                }
            }

            srm.AllGroups = srm.GroupedCartonsToPull.Select(p => p.Key)
                .Concat(srm.GroupedSkuRequirements.Select(p => p.Key))
                .Concat(srm.GroupedCartonsToRestock.Select(p => p.Key))
                .Where(p => !string.IsNullOrWhiteSpace(p.BuildingId))
                .Distinct().ToArray();
            foreach (var group in srm.AllGroups)
            {
                IList<DiagnosticCartonModel> cartons;
                if (srm.GroupedCartonsToRestock.TryGetValue(group, out cartons))
                {
                    group.UpdateRestockTotals(cartons);
                }

                IList<SkuRequirementModel> requirements;
                if (srm.GroupedSkuRequirements.TryGetValue(group, out requirements))
                {
                    group.UpdateRequirementDetails(requirements);
                }

                if (srm.GroupedCartonsToPull.TryGetValue(group, out cartons))
                {
                    group.UpdatePullableCartonCount(cartons);
                }
            }

            return View(Views.SearchSku, srm);
        }

        /// <summary>
        /// Overriding the OnAuthorization() to handle the issue with logged in user to execute those action method which can be called as anonymous user.
        /// Now service will be called with username for those action method which having AuthorizeExAttribute, 
        /// and which have not this attribute those will be called with super user.
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            var connectString = ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString;
            var clientInfo = string.IsNullOrEmpty(this.HttpContext.Request.UserHostName) ? this.HttpContext.Request.UserHostAddress :
                this.HttpContext.Request.UserHostName;

            if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(AuthorizeExAttribute), true).Any())
            {
                // create service with user
                var userName = this.HttpContext.SkipAuthorization ? string.Empty : this.HttpContext.User.Identity.Name;
                _service = new DiagnosticService(userName, clientInfo, this.HttpContext.Trace, connectString);
            }
            else
            {
                // create service without user
                _service = new DiagnosticService(string.Empty, clientInfo, this.HttpContext.Trace, connectString);
            }
            base.OnAuthorization(filterContext);
        }

    }
}
