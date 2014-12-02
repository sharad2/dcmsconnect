using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository.Config;
using DcmsMobile.PickWaves.ViewModels.Config;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Controllers
{
    [AuthorizeEx("Managing Pick Waves requires role {0}", Roles = ROLE_WAVE_MANAGER)]
    [RoutePrefix("config")]
    public partial class ConfigController : PickWavesControllerBase
    {
        #region Intialization
        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public ConfigController()
        {

        }

        private ConfigService _service;

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (_service == null)
            {
                _service = new ConfigService(this.HttpContext.Trace,
                    HttpContext.User.IsInRole(ROLE_WAVE_MANAGER) ? HttpContext.User.Identity.Name : string.Empty,
                    HttpContext.Request.UserHostName ?? HttpContext.Request.UserHostAddress);
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

        #region mapping

        /// <summary>
        /// This function maps from SkuCAse entity to SelectListItem
        /// </summary>
        /// <param name="skuCase"></param>
        /// <returns></returns>
        private SelectListItem Map(SkuCase skuCase)
        {
            return new SelectListItem
            {
                Text = skuCase.CaseId + ":" + skuCase.Description,
                Value = skuCase.CaseId
            };
        }

        /// <summary>
        /// This function maps SKucaseModel into SkuCase entity
        /// </summary>
        /// <param name="skuCase"></param>
        /// <returns></returns>
        private SkuCase MapSkuCase(SkuCaseModel skuCase)
        {
            return new SkuCase
            {
                CaseId = skuCase.CaseId,
                Description = skuCase.Description,
                EmptyWeight = skuCase.EmptyWeight,
                IsAvailable = skuCase.IsAvailable,
                MaxContentVolume = skuCase.MaxContentVolume,
                OuterCubeVolume = skuCase.OuterCubeVolume
            };
        }



        #endregion

        [AllowAnonymous]
        [Route]
        public virtual ActionResult Index()
        {
            var skuCaseList = _service.GetSkuCaseList().ToArray();
            var maxSkuCase = skuCaseList.OrderByDescending(p => p.MaxContentVolume).First();
            var minSkuCase = skuCaseList.OrderBy(p => p.MaxContentVolume).First();
            var model = new IndexViewModel
            {
                MaxCaseId = maxSkuCase.CaseId,
                MaxCaseDescription = maxSkuCase.Description,
                MaxContentVolume = maxSkuCase.MaxContentVolume,
                MinCaseId = minSkuCase.CaseId,
                MinCaseDescription = minSkuCase.Description,
                MinContentVolume = minSkuCase.MaxContentVolume
            };
            return View(Views.Index, model);

        }

        #region Manage SKU Case


        /// <summary>
        /// This function deletes customer SKU case constraint.
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="customerId"></param>
        /// <param name="activeTab">Index of active tab</param>
        /// <returns></returns>
        [HttpPost]
        [Route("delcaseconstraint")]
        public virtual ActionResult DeleteCustomerSkuCaseConstraint(string caseId, string customerId, int? activeTab)
        {
            try
            {
                _service.DelCustSkuCasePrefereence(customerId, caseId);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException);
                return RedirectToAction(Actions.SkuCase(activeTab));
            }
            AddStatusMessage(string.Format("Deleted SKU case {0} from customer {1} preference.", caseId, customerId));
            return RedirectToAction(Actions.SkuCase(activeTab));
        }

        /// <summary>
        /// This function adds customer SKU case preference.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="activeTab">Index of active tab</param>
        /// <returns></returns>
        [HttpPost]
        [Route("addpref")]
        public virtual ActionResult AddCustomerSkuCasePreference(CustomerSkuCaseModel model, int? activeTab)
        {
            try
            {
                _service.AddCustSKuCasePreference(model.CustomerId, model.CaseId, model.Comment);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException);
                return RedirectToAction(Actions.SkuCase(activeTab));
            }
            AddStatusMessage(string.Format("SKU case {0} is added to customer {1} preference.", model.CaseId,
                                                model.CustomerId));
            return RedirectToAction(Actions.SkuCase(activeTab));
        }

        /// <summary>
        /// This function update's properties of Sku case.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("updateskucase")]
        public virtual ActionResult AddOrUpdateSkuCase(SkuCaseModel model)
        {
            //TC1:You will get here if required feild not passed.
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Required fields must be passed");
                return RedirectToAction(Actions.SkuCase());
            }

            try
            {
                _service.AddorUpdateSkuCase(MapSkuCase(model));
                AddStatusMessage(string.Format("Provided SKU Case properties has been set for case {0}.", model.CaseId));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException);
                return RedirectToAction(Actions.SkuCase());
            }
            return RedirectToAction(Actions.SkuCase());
        }

        /// <summary>
        /// This function fetches all SKU Cases, customer preferred SKU cases ,and packing rules for SKU cases and returns SKU case view with last selected tab.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("sku")]
        public virtual ActionResult SkuCase(int? activeTab)
        {
            var skuCases = _service.GetSkuCaseList().ToArray();
            //TC2: If no sku case has been defined yet.
            if (!skuCases.Any())
            {
                AddStatusMessage("SKU case is not added yet.");
                return View(Views.SkuCase, new SkuCaseViewModel());
            }

            var custSkuCaseList = _service.GetCustomerSkuCaseList();
            var packingRules = _service.GetPackingRules();
            var model = new SkuCaseViewModel
                {
                    CustomerSkuCaseList = custSkuCaseList.Select(p => new CustomerSkuCaseModel
                        {
                            CaseId = p.CaseId,
                            CustomerId = p.CustomerId,
                            CustomerName = p.CustomerName,
                            EmptyWeight = p.EmptyWeight,
                            MaxContentVolume = p.MaxContentVolume,
                            OuterCubeVolume = p.OuterCubeVolume,
                            Comment = p.Comment,
                            CaseDescription = p.CaseDescription
                        }).ToList(),

                    SkuCaseList = skuCases.Select(p => new SkuCaseModel(p)).ToList(),

                    PackingRuleList = packingRules.Select(p => new PackingRulesModel
                        {
                            CaseId = p.CaseId,
                            Style = p.Style,
                            IgnoreFlag = p.IgnoreFlag
                        }).ToList(),
                    ActiveTab = activeTab
                };
            return View(Views.SkuCase, model);
        }

        /// <summary>
        /// This function renders Sku case editor partial view.
        /// </summary>
        /// <param name="skuCaseId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("skucaseeditor")]
        public virtual ActionResult GetSkuCaseEditor(string skuCaseId)
        {
            //TC3: If sku case does not passed. this will not come in normal user practise.
            if (string.IsNullOrEmpty(skuCaseId))
            {
                throw new ArgumentNullException("skuCaseId", "Internal error. The SKU case to edit should be specified");
            }

            var skuCase = _service.GetSkuCase(skuCaseId);
            //TC4: You will get here if passed SKU case has been deleted.
            if (skuCase == null)
            {
                throw new ArgumentOutOfRangeException("skuCaseId", string.Format("SKU Case {0} does not exist. It may have been deleted", skuCaseId));
            }

            var html = RenderPartialViewToString(Views._skuCaseEditorPartial, new SkuCaseModel(skuCase));
            return Content(html);
        }

        /// <summary>
        /// This function returns Sku Case Add Partial.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("skucase")]
        public virtual ActionResult SkuCaseAddPartial()
        {

            var html = RenderPartialViewToString(Views._skuCaseEditorPartial, new SkuCaseModel());
            return Content(html);
        }

        /// <summary>
        /// This function returns partial view to add customer's Sku case preferences.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("prefeditor")]
        public virtual ActionResult CustSkuCasePreferenceEditor(string customerId)
        {
            var skuCaseList = _service.GetSkuCaseList();
            var model = new CustomerSkuCaseModel
                {
                    SkuCaseList = skuCaseList.Select(Map),
                    CustomerId = customerId
                };
            var html = RenderPartialViewToString(Views._custSkuCasePreferenceEditorPartial, model);
            return Content(html);
        }

        /// <summary>
        /// This function deletes specific Packing rule.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="caseId"></param>
        /// <param name="activeTab"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("delpackingrule")]
        public virtual ActionResult DelPackingRule(string style, string caseId, int? activeTab)
        {
            try
            {
                _service.DelCaseIgnorance(style, caseId);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException);
                return RedirectToAction(Actions.SkuCase(activeTab));
            }
            AddStatusMessage(string.Format("Ignorance of case {0} is deleted against SKU {1}", caseId, style));
            return RedirectToAction(Actions.SkuCase(activeTab));
        }

        /// <summary>
        /// This function returns partial view to add a new packing rule.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("addview")]
        public virtual ActionResult PackingRuleAddView(string style, string caseId, bool? ignoreFlag)
        {
            var skuCaseList = _service.GetSkuCaseList();
            var model = new PackingRulesModel
            {
                SkuCaseList = skuCaseList.Select(Map),
                Style = style,
                CaseId = caseId,
                IgnoreFlag = ignoreFlag.HasValue
            };
            var html = RenderPartialViewToString(Views._addPackinRulePartial, model);
            return Content(html);
        }

        /// <summary>
        /// This function adds a new packing rule.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="activeTab"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("updaterule")]
        public virtual ActionResult AddPackingRule(PackingRulesModel model, int? activeTab)
        {
            //TC5: If required feild does not passed.
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Required fields must be passed");
                return RedirectToAction(Actions.SkuCase());
            }
            try
            {
                _service.InsertPackingRule(new PackingRules
                {
                    CaseId = model.CaseId,
                    IgnoreFlag = model.IgnoreFlag,
                    Style = model.Style
                });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException);
                return RedirectToAction(Actions.SkuCase(activeTab));
            }
            this.AddStatusMessage(string.Format("Packing rule is added for case {0} against style {1}", model.CaseId, model.Style));
            return RedirectToAction(Actions.SkuCase(activeTab));
        }
        #endregion

        /// <summary>
        /// Returns Constraint view.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("constraint")]
        public virtual ActionResult Constraint(int? selectedTab)
        {
            var constraints = _service.GetDefaultConstraints();
            var custConstraints = _service.GetAllCustomerConstraints();
            var model = new ConstraintViewModel
            {
                DefaultConstraints = new ConstraintModel(constraints),
                ActiveTab = selectedTab ?? 0
            };

            //Getting SPLH setting group by Customer.
            var query = from item in custConstraints
                        orderby item.Key.Name
                        select new
                        {
                            Customer = item.Key,
                            Constraint = new ConstraintModel(item.Value)
                        };

            foreach (var item in query.Where(p => p.Constraint.HasConstraint))
            {
                model.CustomerGroupedList.Add(new CustomerModel
                {
                    CustomerId = item.Customer.CustomerId,
                    CustomerName = item.Customer.Name
                }, item.Constraint);
            }
            return View(Views.Constraint, model);
        }

        /// <summary>
        /// To edit passed customer's constraint.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [Route("editview")]
        public virtual ActionResult CustomerConstraintEditView(string customerId)
        {
            //TC1: Invalid customer Id passed.
            if (string.IsNullOrEmpty(customerId))
            {
                throw new ArgumentNullException("customerId", "Internal error. The Customer to edit should be specified");
            }
            string customerName;
            var customerConstraints = _service.GetCustomerConstraints(customerId, out customerName);
            //TC2: If no constraint defined for passed customer. This haapnned only when when some one deleted customer.
            if (customerConstraints == null)
            {
                throw new ArgumentOutOfRangeException("customerId", string.Format("Customer {0} does not exist. It may have been deleted", customerId));
            }
            var model = new CustomerConstraintEditorModel(customerConstraints)
                {
                    CustomerId = customerId,
                    CustomerName = customerName
                };
            var html = RenderPartialViewToString(Views._addCustomerConstraintPartial, model);
            return Content(html);
        }

        /// <summary>
        /// Add new customer and its constraint.
        /// </summary>
        /// <returns></returns>
        [Route("addview")]
        public virtual ActionResult CustomerConstraintAddView()
        {
            var html = RenderPartialViewToString(Views._addCustomerConstraintPartial, new CustomerConstraintEditorModel());
            return Content(html);
        }


        [HttpPost]
        [Route("updatecust")]
        public virtual ActionResult UpdateCustomerConstraints(string customerId, CustomerConstraintEditorModel constraints, int? activeTab)
        {
            //TC3: You will get here if required feild does not passed.
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Required fields must be passed");
                return RedirectToAction(Actions.Constraint(activeTab));
            }
            //TC4:You will get here if user update min pieces more than max sku pieces.
            if (constraints.OrigRequiredMinSkuPieces != constraints.RequiredMinSkuPieces || constraints.OrigRequiredMaxSkuPieces != constraints.RequiredMaxSkuPieces)
            {
                if ((constraints.RequiredMinSkuPieces > constraints.OrigRequiredMaxSkuPieces && constraints.RequiredMinSkuPieces > constraints.RequiredMaxSkuPieces) || constraints.RequiredMinSkuPieces > constraints.RequiredMaxSkuPieces)
                {
                    ModelState.AddModelError("", "Max SKU pieces must be greater than or equal to Min SKU pieces.");
                    return RedirectToAction(Actions.Constraint(activeTab));
                }
                _service.UpdateSkuMinMaxPieces(customerId, constraints.RequiredMinSkuPieces, constraints.RequiredMaxSkuPieces, constraints.OrigRequiredMaxSkuPieces, constraints.OrigRequiredMinSkuPieces);
            }
            //TC5: Update Max box weight value only if is different from its old value.
            if (constraints.OrigMaxBoxWeight != constraints.MaxBoxWeight)
            {
                _service.UpdateMaxBoxWeight(customerId, constraints.MaxBoxWeight);
            }
            //TC6: Upate MaxSkuWithinBox value only if is different from its old value.
            if (constraints.OrigMaxSkuWithinBox != constraints.MaxSkuWithinBox)
            {
                _service.UpdateMaxSkuInBox(customerId, constraints.MaxSkuWithinBox);
            }
            //TC7: Update single style color value only if is different from its old value.
            if (constraints.OrigIsSingleStyleColor != constraints.IsSingleStyleColor)
            {
                _service.UpdateSkuMixing(customerId, constraints.IsSingleStyleColor);
            }
            AddStatusMessage(string.Format("Provided setting has been set for the Customer: {0}.", customerId));
            return RedirectToAction(Actions.Constraint(activeTab));
        }

        protected override string ManagerRoleName
        {
            get { return ROLE_WAVE_MANAGER; }
        }
    }
}
