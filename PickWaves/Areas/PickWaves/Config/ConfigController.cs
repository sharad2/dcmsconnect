using DcmsMobile.PickWaves.Helpers;
using EclipseLibrary.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Config
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

        private Lazy<ConfigService> _service;

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            _service = new Lazy<ConfigService>(() => new ConfigService(this.HttpContext.Trace,
                HttpContext.User.IsInRole(ROLE_WAVE_MANAGER) ? HttpContext.User.Identity.Name : string.Empty,
                HttpContext.Request.UserHostName ?? HttpContext.Request.UserHostAddress));


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

        ///// <summary>
        ///// This function maps SKucaseModel into SkuCase entity
        ///// </summary>
        ///// <param name="skuCase"></param>
        ///// <returns></returns>
        //[Obsolete]
        //private SkuCase MapSkuCase(SkuCaseModel skuCase)
        //{
        //    return new SkuCase
        //    {
        //        CaseId = skuCase.CaseId,
        //        Description = skuCase.Description,
        //        EmptyWeight = skuCase.EmptyWeight,
        //        IsAvailable = skuCase.IsAvailable,
        //        MaxContentVolume = skuCase.MaxContentVolume,
        //        OuterCubeVolume = skuCase.OuterCubeVolume
        //    };
        //}



        #endregion

        [AllowAnonymous]
        [Route]
        public virtual ActionResult Index()
        {
            var skuCaseList = _service.Value.GetSkuCaseList().ToArray();
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

        #region CustSkuCase

        /// <summary>
        /// This function fetches all SKU Cases, customer preferred SKU cases ,and packing rules for SKU cases and returns SKU case view with last selected tab.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("custskucase")]
        public virtual ActionResult CustSkuCase()
        {
            var custSkuCaseList = _service.Value.GetCustomerSkuCaseList();
            //var packingRules = _service.Value.GetPackingRules();
            var model = new CustSkuCaseViewModel
            {
                CustomerSkuCaseList = custSkuCaseList.Select(p => new CustSkuCaseModel
                {
                    CaseId = p.CaseId,
                    CustomerId = p.CustomerId,
                    CustomerName = p.CustomerName,
                    EmptyWeight = p.EmptyWeight,
                    MaxContentVolume = p.MaxContentVolume,
                    OuterCubeVolume = p.OuterCubeVolume,
                    Comment = p.Comment,
                    CaseDescription = p.CaseDescription
                }).ToList()
            };
            return View(Views.CustSkuCase, model);
        }

        /// <summary>
        /// This function returns partial view to add customer's Sku case preferences.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("editor/custskucase")]
        public virtual ActionResult CustSkuCaseEditor(string customerId)
        {
            var skuCaseList = _service.Value.GetSkuCaseList();
            var model = new CustSkuCaseEditorViewModel
            {
                SkuCaseList = skuCaseList.Select(Map),
                CustomerId = customerId
            };
            return PartialView(Views._custSkuCaseEditorPartial, model);
        }

        /// <summary>
        /// This function deletes customer SKU case constraint.
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="customerId"></param>
        /// <param name="activeTab">Index of active tab</param>
        /// <returns></returns>
        [HttpPost]
        [Route("delcustskucase")]
        public virtual ActionResult DelCustSkuCase(string caseId, string customerId)
        {
            try
            {
                _service.Value.DelCustSkuCasePrefereence(customerId, caseId);
                AddStatusMessage(string.Format("Deleted SKU case {0} from customer {1} preference.", caseId, customerId));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException);
            }
            return RedirectToAction(Actions.CustSkuCase());
        }

        /// <summary>
        /// This function adds customer SKU case preference.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="activeTab">Index of active tab</param>
        /// <returns></returns>
        [HttpPost]
        [Route("addcustskucase")]
        public virtual ActionResult AddCustSkuCase(CustSkuCaseModel model)
        {
            try
            {
                _service.Value.AddCustSKuCasePreference(model.CustomerId, model.CaseId, model.Comment);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException);
                return RedirectToAction(Actions.SkuCase());
            }
            AddStatusMessage(string.Format("SKU case {0} is added to customer {1} preference.", model.CaseId,
                                                model.CustomerId));
            return RedirectToAction(Actions.CustSkuCase());
        }

        #endregion

        #region StyleSkuCase
        [Route("styleskucase")]
        public virtual ActionResult StyleSkuCase()
        {
            var packingRules = _service.Value.GetPackingRules();
            //var model = new StyleSkuCaseViewModel
            //{
            //    PackingRuleList = packingRules.Select(p => new StyleSkuCaseModel
            //    {
            //        CaseId = p.CaseId,
            //        Style = p.Style,
            //        IgnoreFlag = p.IgnoreFlag
            //    }).ToList()
            //};

            var query = from rule in packingRules
                        group rule by rule.Style into g
                        orderby g.Key
                        select new StyleSkuCaseModel
                        {
                            Style = g.Key,
                            StyleCases = new SortedList<string, bool>(g.ToDictionary(p => p.CaseId, p => p.IgnoreFlag))
                        };

            var model = new StyleSkuCaseViewModel
            {
                PackingRuleList = query.ToList()
            };

            return View(Views.StyleSkuCase, model);
        }

        /// <summary>
        /// This function returns partial view to add a new packing rule.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("editor/stylesku")]
        public virtual ActionResult StyleSkuCaseEditor(string style, string caseId, bool? ignoreFlag)
        {
            var skuCaseList = _service.Value.GetSkuCaseList();
            var model = new StyleSkuCaseEditorViewModel
            {
                SkuCaseList = skuCaseList.Select(Map),
                Style = style,
                CaseId = caseId,
                IgnoreFlag = ignoreFlag.HasValue
            };
            return PartialView(Views._styleSkuCaseEditorPartial, model);
        }

        /// <summary>
        /// This function adds a new packing rule.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="activeTab"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("updaterule")]
        public virtual ActionResult UpdateStyleSkuCase(StyleSkuCaseEditorViewModel model)
        {
            //TC5: If required feild does not passed.
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Required fields must be passed");
                //return RedirectToAction(Actions.SkuCase());
                return RedirectToAction(Actions.StyleSkuCase());
            }
            try
            {
                _service.Value.InsertPackingRule(new StyleSkuCase
                {
                    CaseId = model.CaseId,
                    IgnoreFlag = model.IgnoreFlag,
                    Style = model.Style
                });
                this.AddStatusMessage(string.Format("Packing rule is added for case {0} against style {1}", model.CaseId, model.Style));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException);
            }

            //return RedirectToAction(Actions.SkuCase());
            return RedirectToAction(Actions.StyleSkuCase());
        }

        /// <summary>
        /// This function deletes specific Packing rule.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="caseId"></param>
        /// <param name="disable">If this is passed as true then the style case is disabled, not deleted</param>
        /// <returns></returns>
        [HttpPost]
        [Route("delpackingrule")]
        public virtual ActionResult DelStyleSkuCase(string style, string caseId, bool? disable)
        {
                        
            try
            {
                if (disable.HasValue)
                {
                    _service.Value.InsertPackingRule(new StyleSkuCase
                    {
                        CaseId = caseId,
                        IgnoreFlag = disable == true ? true : false,
                        Style = style
                    });
                    AddStatusMessage(string.Format("Case {0} ignorance modified against SKU {1}", caseId, style));
                }
                else
                {
                    _service.Value.DelCaseIgnorance(style, caseId);
                    AddStatusMessage(string.Format("Ignorance of case {0} is deleted against SKU {1}", caseId, style));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException);              
            }

            return RedirectToAction(Actions.StyleSkuCase());
        }

        #endregion

        #region Manage SKU Case
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
                _service.Value.AddorUpdateSkuCase(
                    new SkuCase
                    {
                        CaseId = model.CaseId,
                        Description = model.Description,
                        EmptyWeight = model.EmptyWeight,
                        IsAvailable = model.IsAvailable,
                        MaxContentVolume = model.MaxContentVolume,
                        OuterCubeVolume = model.OuterCubeVolume
                    });
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
        [Route("skucase")]
        public virtual ActionResult SkuCase()
        {
            var skuCases = _service.Value.GetSkuCaseList().ToArray();
            //TC2: If no sku case has been defined yet.
            if (!skuCases.Any())
            {
                AddStatusMessage("SKU case is not added yet.");
                return View(Views.SkuCase, new SkuCaseViewModel());
            }
            //var packingRules = _service.Value.GetPackingRules();
            var model = new SkuCaseViewModel
                {
                    SkuCaseList = skuCases.Select(p => new SkuCaseModel(p)).ToList()
                };
            return View(Views.SkuCase, model);
        }

        /// <summary>
        /// This function renders Sku case editor partial view.
        /// </summary>
        /// <param name="skuCaseId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("editor/skucase")]
        public virtual ActionResult SkuCaseEditor(string skuCaseId)
        {
            SkuCaseModel model;
            if (string.IsNullOrWhiteSpace(skuCaseId))
            {
                model = new SkuCaseModel();
            }
            else
            {
                var skuCase = _service.Value.GetSkuCase(skuCaseId);
                //TC4: You will get here if passed SKU case has been deleted.
                if (skuCase == null)
                {
                    throw new ArgumentOutOfRangeException("skuCaseId", string.Format("SKU Case {0} does not exist. It may have been deleted", skuCaseId));
                }
                model = new SkuCaseModel(skuCase);
            }

            //var html = RenderPartialViewToString(Views._skuCaseEditorPartial, new SkuCaseModel(skuCase));
            return PartialView(Views._skuCaseEditorPartial, model);
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
            var constraints = _service.Value.GetDefaultConstraints();
            var custConstraints = _service.Value.GetAllCustomerConstraints();
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
        [Route("editor/custconstraint")]
        public virtual ActionResult CustomerConstraintEditor(string customerId)
        {
            CustomerConstraintEditorModel model;
            if (string.IsNullOrEmpty(customerId))
            {
                model = new CustomerConstraintEditorModel();
            }
            else
            {
                string customerName;
                var customerConstraints = _service.Value.GetCustomerConstraints(customerId, out customerName);
                //TC2: If no constraint defined for passed customer. This haapnned only when when some one deleted customer.
                if (customerConstraints == null)
                {
                    throw new ArgumentOutOfRangeException("customerId", string.Format("Customer {0} does not exist. It may have been deleted", customerId));
                }
                model = new CustomerConstraintEditorModel(customerConstraints)
                    {
                        CustomerId = customerId,
                        CustomerName = customerName
                    };
            }
            return PartialView(Views._customerConstraintEditorPartial, model);
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
                _service.Value.UpdateSkuMinMaxPieces(customerId, constraints.RequiredMinSkuPieces, constraints.RequiredMaxSkuPieces, constraints.OrigRequiredMaxSkuPieces, constraints.OrigRequiredMinSkuPieces);
            }
            //TC5: Update Max box weight value only if is different from its old value.
            if (constraints.OrigMaxBoxWeight != constraints.MaxBoxWeight)
            {
                _service.Value.UpdateMaxBoxWeight(customerId, constraints.MaxBoxWeight);
            }
            //TC6: Upate MaxSkuWithinBox value only if is different from its old value.
            if (constraints.OrigMaxSkuWithinBox != constraints.MaxSkuWithinBox)
            {
                _service.Value.UpdateMaxSkuInBox(customerId, constraints.MaxSkuWithinBox);
            }
            //TC7: Update single style color value only if is different from its old value.
            if (constraints.OrigIsSingleStyleColor != constraints.IsSingleStyleColor)
            {
                _service.Value.UpdateSkuMixing(customerId, constraints.IsSingleStyleColor);
            }
            AddStatusMessage(string.Format("Provided setting has been set for the Customer: {0}.", customerId));
            return RedirectToAction(Actions.Constraint(activeTab));
        }

        protected override string ManagerRoleName
        {
            get { return ROLE_WAVE_MANAGER; }
        }





        #region autocomplete
        /// <summary>
        /// method for Autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        [Route("custautocomplete")]
        public virtual ActionResult CustomerAutocomplete(string term)
        {
            // Change null to empty string
            term = term ?? string.Empty;

            var tokens = term.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            string searchId;
            string searchDescription;

            switch (tokens.Count)
            {
                case 0:
                    // All customer
                    searchId = searchDescription = string.Empty;
                    break;

                case 1:
                    // Try to match term with either id or description
                    searchId = searchDescription = tokens[0];
                    break;

                case 2:
                    // Try to match first token with id and second with description
                    searchId = tokens[0];
                    searchDescription = tokens[1];
                    break;

                default:
                    // For now, ignore everything after the second :
                    searchId = tokens[0];
                    searchDescription = tokens[1];
                    break;


            }
            var data = _service.Value.CustomerAutoComplete(searchId, searchDescription).Select(p => new
            {
                label = string.Format("{0}: {1}", p.Item1, p.Item2),
                value = p.Item1
            }); ;

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Route("styleautocomplete")]
        public virtual ActionResult StyleAutoComplete(string term)
        {
            // Change null to empty string
            term = term ?? string.Empty;

            var tokens = term.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            string searchId;
            string searchDescription;

            switch (tokens.Count)
            {
                case 0:
                    // All style
                    searchId = searchDescription = string.Empty;
                    break;

                case 1:
                    // Try to match term with either id or description
                    searchId = searchDescription = tokens[0];
                    break;

                case 2:
                    // Try to match first token with id and second with description
                    searchId = tokens[0];
                    searchDescription = tokens[1];
                    break;

                default:
                    // For now, ignore everything after the second :
                    searchId = tokens[0];
                    searchDescription = tokens[1];
                    break;


            }
            var data = _service.Value.StyleAutoComplete(searchId, searchDescription).Select(p => new
            {
                label = string.Format("{0}: {1}", p.Item1, p.Item2),
                value = p.Item1
            }); ;

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
