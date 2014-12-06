using DcmsMobile.BoxManager.Repository.VasConfiguration;
using DcmsMobile.BoxManager.ViewModels.VasConfiguration;
using EclipseLibrary.Mvc.Controllers;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.BoxManager.Areas.BoxManager.Controllers
{
    [RouteArea("BoxManager")]
    [RoutePrefix(VasConfigurationController.NameConst)]
    public partial class VasConfigurationController : EclipseController
    {
        #region Initialize

        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public VasConfigurationController()
        {

        }

        private VasConfigurationService _service;

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (_service == null)
            {
                var connectString = ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString;
                var userName = requestContext.HttpContext.SkipAuthorization ? string.Empty : requestContext.HttpContext.User.Identity.Name;
                var clientInfo = string.IsNullOrEmpty(requestContext.HttpContext.Request.UserHostName) ? requestContext.HttpContext.Request.UserHostAddress :
                    requestContext.HttpContext.Request.UserHostName;
                _service = new VasConfigurationService(requestContext.HttpContext.Trace, connectString, userName, clientInfo, "V2P");
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

        public const string VAS_CONFIGURATION_ROLE = "DCMS8_VAS";

        /// <summary>
        /// Populate the Index page of VAS Configuration UI
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// It is possible to pass CustomerId and VasId in the model. This will focus the UI on the passed VAS for the passed customer
        /// </remarks>
        [Route(VasConfigurationController.ActionNameConstants.Index, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_VASConfigration)]
        public virtual ActionResult Index()
        {
            var model = new IndexViewModel();
            var settings = _service.GetCustomerVasSettings(null).OrderBy(p => p.CustomerName).ThenBy(p => p.VasDescription);
            model.VasSettingList = settings.Select(item => new CustomerVasSettingModel
            {
                CustomerId = item.CustomerId,
                CustomerName = item.CustomerName,
                VasId = item.VasId,
                VasDescription = item.VasDescription,
                VasPatternDescription = item.VasPatternDescription,
                Remark = item.Remark,
                InactiveFlag = item.InactiveFlag
            }).ToList();

            var query = from item in settings
                        group item by item.VasId
                            into g
                            select (new
                                {
                                    Key = g.Key,
                                    Values = g.Select(item => new CustomerVasSettingModel
                                        {
                                            CustomerId = item.CustomerId,
                                            CustomerName = item.CustomerName,
                                            VasDescription = item.VasDescription,
                                            VasPatternDescription = item.VasPatternDescription,
                                            Remark = item.Remark,
                                            InactiveFlag = item.InactiveFlag
                                        })
                                });

            foreach (var item in query)
            {
                model.VasGroupedList.Add(item.Key, item.Values.ToList());
            }

            model.VasCodeList = from item in _service.GetVasList()
                                select new SelectListItem
                                    {
                                        Text = item.Description,
                                        Value = item.Code
                                    };
            return View(Views.Index, model);
        }

        /// <summary>
        /// Populates the sidebar menu.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vasId"></param>
        /// <returns></returns>
        public virtual ActionResult CustomerList(string customerId, string vasId)
        {
            var query = from item in _service.GetCustomerVasSettings(null)
                        orderby item.CustomerName, item.VasDescription
                        group item by item.CustomerId into g
                        select new
                        {
                            g.Key,
                            Values = g.Select(item => new CustomerVasSettingModel
                            {
                                CustomerId = item.CustomerId,
                                CustomerName = item.CustomerName,
                                VasId = item.VasId,
                                VasDescription = item.VasDescription,
                                VasPatternDescription = item.VasPatternDescription,
                                Remark = item.Remark,
                                InactiveFlag = item.InactiveFlag
                            })
                        };
            var model = new CustomerGroupedListModel
                {
                    CustomerId = customerId,
                    VasId = vasId
                };
            foreach (var item in query)
            {
                model.CustomerGroupedList.Add(item.Key, item.Values.ToList());
            }
            return PartialView(Views._menuPartial, model);
        }


        [AuthorizeEx("VAS Configuration requires Role {0}", Roles = "DCMS8_VAS")]
        public virtual ActionResult CustomerVas(string customerId, string vasId)
        {
            var customerVasList = _service.GetCustomerVasSettings(customerId).ToArray();
            if (customerVasList.All(p => p.VasId != vasId))
            {
                ModelState.AddModelError("", string.Format("VAS setting for customer {0} is already removed.", customerId));
                return RedirectToAction(Actions.Index());
            }
            var customerVasSetting = customerVasList.First(p => p.VasId == vasId);
            var model = new CustomerVasViewModel
                {
                    CustomerId = customerId,
                    CustomerName = customerVasSetting.CustomerName,
                    VasId = vasId,
                    VasDescription = customerVasSetting.VasDescription,
                    UserRemarks = customerVasSetting.Remark,
                    VasPatternDescription = customerVasSetting.VasPatternDescription,
                    InactiveFlag = customerVasSetting.InactiveFlag
                };
            return View(this.Views.CustomerVas, model);
        }

        [AuthorizeEx("VAS Configuration requires Role {0}", Roles = "DCMS8_VAS")]
        public virtual ActionResult EditCustomerVas(string customerId, string vasId)
        {
            var customerVasList = _service.GetCustomerVasSettings(customerId).ToArray();
            if (customerVasList.All(p => p.VasId != vasId))
            {
                ModelState.AddModelError("", string.Format("VAS setting for customer {0} is already removed.", customerId));
                return RedirectToAction(Actions.Index());
            }
            var customerVasSetting = customerVasList.First(p => p.VasId == vasId);

            var model = new EditCustomerVasViewModel
                {
                    CustomerId = customerId,
                    CustomerName = customerVasSetting.CustomerName,
                    VasId = vasId,
                    VasDescription = customerVasSetting.VasDescription,
                    RegExDescription = customerVasSetting.VasPatternDescription,
                };
            return View(Views.EditCustomerVas, model);
        }

        [AuthorizeEx("VAS Configuration requires Role {0}", Roles = "DCMS8_VAS")]
        public virtual ActionResult VerifyVasPattern(EditCustomerVasViewModel model)
        {
            var customerVasList = _service.GetCustomerVasSettings(model.CustomerId).ToArray();
            if (customerVasList.All(p => p.VasId != model.VasId))
            {
                ModelState.AddModelError("", string.Format("VAS setting for customer {0} is already removed.", model.CustomerId));
                return RedirectToAction(Actions.Index());
            }
            var customerVasSetting = customerVasList.First(p => p.VasId == model.VasId);

            var newModel = new VerifyVasPatternViewModel
                {
                    CustomerId = model.CustomerId,
                    VasId = model.VasId,
                    CustomerName = customerVasSetting.CustomerName,
                    VasDescription = customerVasSetting.VasDescription,
                    PatternRegEx = customerVasSetting.PatternRegEx,
                    RegExDescription = customerVasSetting.VasPatternDescription,
                    PoPatternType = model.DoApplyPoPattern ? model.PoPatternType : null,
                    PoTextType = model.DoApplyPoPattern ? model.PoTextType : null,
                    Labels = model.Labels,
                    PoText = model.DoApplyPoPattern && model.PoTextType == null ? model.PoText : string.Empty
                };
            var query = _service.GetComprehensivePoList(model.CustomerId,model.VasId, customerVasSetting.PatternRegEx, newModel.ConstructedRegEx).ToArray();
            newModel.ListWillApply = (from item in query
                                      where item.Item1 == VasConfigurationRepository.PoQualificationType.NewOnly || item.Item1 == VasConfigurationRepository.PoQualificationType.BothOldAndNew
                                      select new VasSettingChangeModel
                                              {
                                                  IsNew = item.Item1 == VasConfigurationRepository.PoQualificationType.NewOnly,
                                                  PoId = item.Item2
                                              }).OrderBy(p => p.IsNew).ToArray();
            newModel.ListWillNotApply = (from item in query
                                         where item.Item1 == VasConfigurationRepository.PoQualificationType.OldOnly || item.Item1 == VasConfigurationRepository.PoQualificationType.NeitherOldNorNew
                                         select new VasSettingChangeModel
                                                 {
                                                     IsNew = item.Item1 == VasConfigurationRepository.PoQualificationType.OldOnly,
                                                     PoId = item.Item2
                                                 }).OrderBy(p => p.IsNew).ToArray();
            return View(Views.VerifyVasPattern, newModel);
        }

        /// <summary>
        /// Add new VAS configuration with default setting parameters.
        /// </summary>
        /// <param name="ivm"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("VAS Configuration requires Role {0}", Roles = "DCMS8_VAS")]
        public virtual ActionResult AddNewVasConfiguration(IndexViewModel ivm)
        {
            //TC: If VAS was already added for the customer, show warning message.
            if (_service.GetCustomerVasSettings(ivm.CustomerId).Any(p => p.VasId == ivm.VasId))
            {
                AddStatusMessage(string.Format("VAS setting already exists for customer {0}.", ivm.CustomerId));
            }
            else
            {
                //TC: New VAS configuration should be added for the customer, VAS Id with default settings.
                _service.AddVasConfiguration(ivm.CustomerId, ivm.VasId);
                AddStatusMessage(string.Format("VAS setting added successfully for the customer {0}.", ivm.CustomerId));
            }
            return RedirectToAction(Actions.CustomerVas(ivm.CustomerId, ivm.VasId));
        }

        /// <summary>
        /// Updates the remarks for VAS configuration entered by user.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("VAS Configuration requires Role {0}", Roles = "DCMS8_VAS")]
        public virtual ActionResult UpdateConfigurationRemark(CustomerVasViewModel model)
        {
            //TC: Check VAS pattern RegEx, RegExDescription, InactiveFlag should not be modified here, Only remark column should be updated.
            _service.UpdateConfigurationRemark(model.CustomerId, model.VasId, model.UserRemarks, model.InactiveFlag);
            AddStatusMessage(string.Format("Remarks updated successfully for the customer {0}.", model.CustomerId));
            return RedirectToAction(Actions.CustomerVas(model.CustomerId, model.VasId));
        }

        /// <summary>
        /// Updates VAS configuration with given new details, Only RegEx pattern and RegEx Pattern Description can be changed here.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("VAS Configuration requires Role {0}", Roles = "DCMS8_VAS")]
        public virtual ActionResult UpdateConfiguration(EditCustomerVasViewModel model)
        {
            //TC: VAS should be updated properly with given new details, Only RegEx pattern and RegEx Pattern Description should change here.
            _service.UpdateVasConfiguration(model.CustomerId, model.VasId, model.ConstructedRegEx, model.ConstructedRegExDescription, null, true);
            AddStatusMessage(string.Format("New settings updated successfully for the customer {0}.", model.CustomerId));
            return RedirectToAction(Actions.CustomerVas(model.CustomerId, model.VasId));
        }

        /// <summary>
        /// Remove the existing VAS Configuration of specific customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vasId"></param>
        /// <returns></returns>
        [AuthorizeEx("VAS Configuration requires Role {0}", Roles = "DCMS8_VAS")]
        public virtual ActionResult RemoveVasConfiguration(string customerId, string vasId)
        {
            var isdone = _service.RemoveVasConfiguration(customerId, vasId);
            //TC2: Check if VAS pattern is not removed, proper message is being shown
            if (isdone)
            {
                AddStatusMessage(string.Format("VAS setting for the customer {0}, removed successfully.", customerId));
            }
            else
            {
                ModelState.AddModelError("", string.Format("Can not remove VAS setting of customer {0}.", customerId));
            }
            return RedirectToAction(Actions.Index());
        }

        [AuthorizeEx("VAS Configuration requires Role {0}", Roles = "DCMS8_VAS")]
        public virtual ActionResult DisableVasConfiguration(string customerId, string vasId)
        {
            var customerVasList = _service.GetCustomerVasSettings(customerId).ToArray();
            if (customerVasList.All(p => p.VasId != vasId))
            {
                ModelState.AddModelError("", string.Format("VAS setting for customer {0} is already removed.", customerId));
                return RedirectToAction(Actions.Index());
            }
            var customerVasSetting = customerVasList.First(p => p.VasId == vasId);

            var model = new DisableVasConfigurationViewModel
            {
                CustomerId = customerId,
                CustomerName = customerVasSetting.CustomerName,
                VasId = vasId,
                VasDescription = customerVasSetting.VasDescription,
                UserRemarks = customerVasSetting.Remark,
                VasPatternDescription = customerVasSetting.VasPatternDescription,
                PatternRegEx = customerVasSetting.PatternRegEx,
                InactiveFlag = customerVasSetting.InactiveFlag,
                ListBeingApplied = _service.GetQualifyingCustomerPos(customerId, vasId, customerVasSetting.PatternRegEx).ToArray()
            };
            return View(Views.DisableVasConfiguration, model);
        }

        /// <summary>
        /// Disables VAS Configuration, on the basis of user's choice.
        /// User can disable all/current/all excluding current orders.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("VAS Configuration requires Role {0}", Roles = "DCMS8_VAS")]
        public virtual ActionResult DisableVasConfiguration(DisableVasConfigurationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(Actions.CustomerVas(model.CustomerId, model.VasId));
            }
            var message = string.Format("VAS Configuration of customer: <em>{0}</em> is disabled for ", model.CustomerId);
            switch (model.OrderType)
            {
                case OrderType.AllOrders:
                    _service.DisableVasConfiguration(model.CustomerId, model.VasId, model.PatternRegEx, null);
                    message += "<em>all orders.</em>";
                    break;
                case OrderType.CurrentOrdersOnly:
                    _service.DisableVasConfiguration(model.CustomerId, model.VasId, model.PatternRegEx, true);
                    message += "<em>current orders only.</em>";
                    break;
                case OrderType.AllExcludingCurrentOrders:
                    _service.DisableVasConfiguration(model.CustomerId, model.VasId, model.PatternRegEx, false);
                    message += "<em>all orders except current orders.</em>";
                    break;
                default:
                    throw new NotImplementedException();
            }
            AddStatusMessage(message);
            return RedirectToAction(Actions.CustomerVas(model.CustomerId, model.VasId));
        }

        [AuthorizeEx("VAS Configuration requires Role {0}", Roles = "DCMS8_VAS")]
        public virtual ActionResult EnableVasConfiguration(string customerId, string vasId)
        {
            var customerVasList = _service.GetCustomerVasSettings(customerId).ToArray();
            if (customerVasList.All(p => p.VasId != vasId))
            {
                ModelState.AddModelError("", string.Format("VAS setting for customer {0} is already removed.", customerId));
                return RedirectToAction(Actions.Index());
            }
            var customerVasSetting = customerVasList.First(p => p.VasId == vasId);

            var model = new EnableVasConfigurationViewModel
            {
                CustomerId = customerId,
                CustomerName = customerVasSetting.CustomerName,
                VasId = vasId,
                VasDescription = customerVasSetting.VasDescription,
                UserRemarks = customerVasSetting.Remark,
                VasPatternDescription = customerVasSetting.VasPatternDescription,
                InactiveFlag = customerVasSetting.InactiveFlag,
                PatternRegEx = customerVasSetting.PatternRegEx,
                ListBeingApplied = _service.GetQualifyingCustomerPos(customerId, vasId, customerVasSetting.PatternRegEx).ToArray()
            };
            return View(Views.EnableVasConfiguration, model);
        }

        /// <summary>
        /// Enables VAS Configuration, on the basis of user's choice.
        /// User can disable all/current/all excluding current orders.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("VAS Configuration requires Role {0}", Roles = "DCMS8_VAS")]
        public virtual ActionResult EnableVasConfiguration(EnableVasConfigurationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(Actions.CustomerVas(model.CustomerId, model.VasId));
            }
            var message = string.Format("VAS Configuration of customer: <em>{0}</em> is enabled for ", model.CustomerId);
            switch (model.OrderType)
            {
                case OrderType.AllOrders:
                    _service.EnableVasConfiguration(model.CustomerId, model.VasId, model.PatternRegEx, true);
                    message += "<em>all orders.</em>";
                    break;
                case OrderType.AllExcludingCurrentOrders:
                    _service.EnableVasConfiguration(model.CustomerId, model.VasId, model.PatternRegEx, false);
                    message += "<em>all orders except current orders.</em>";
                    break;
                default:
                    throw new NotImplementedException();
            }
            AddStatusMessage(message);
            return RedirectToAction(Actions.CustomerVas(model.CustomerId, model.VasId));
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var vr = filterContext.Result as ViewResult;
            if (vr != null)
            {
                var model = vr.Model as VasConfigurationViewModelBase;
                if (model != null)
                {
                    model.IsEditable = AuthorizeExAttribute.IsSuperUser(HttpContext) || HttpContext.User.IsInRole(VAS_CONFIGURATION_ROLE);
                    model.EditableRoleName = VAS_CONFIGURATION_ROLE;
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
                result = RedirectToActionPermanent(MVC_BoxManager.BoxManager.Home.Index());
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
                        result = RedirectToAction(this.Actions.Index());
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
