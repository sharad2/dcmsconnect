using DcmsMobile.PieceReplenish.Repository.Restock;
using DcmsMobile.PieceReplenish.ViewModels.Restock;
using EclipseLibrary.Mvc.Controllers;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;


namespace DcmsMobile.PieceReplenish.Areas.PieceReplenish.Controllers
{
    [AuthorizeEx("Restock requires Role {0}", Roles = "DCMS8_Restock")]
    [RouteArea("PieceReplenish")]
    [RoutePrefix(RestockController.NameConst)]
    public partial class RestockController : EclipseController
    {
        #region Intialization

        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public RestockController()
        {

        }

        private RestockService _service;

        protected override void Initialize(RequestContext requestContext)
        {

            var connectString = ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString;

            if (_service == null)
            {
                _service = new RestockService(requestContext, requestContext.HttpContext.User.Identity.Name, connectString);
            }
            base.Initialize(requestContext);
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
        /// Just before redirecting save in temp data whether we have errors
        /// </summary>
        /// <param name="routeName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        protected override RedirectToRouteResult RedirectToRoute(string routeName, RouteValueDictionary routeValues)
        {
            // Save the error state in TempData
            this.TempData["PieceReplenish_ModelErrors"] = ModelState.IsValid;
            return base.RedirectToRoute(routeName, routeValues);
        }

        /// <summary>
        /// Before displaying, set the sound based on temp data error state
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="masterName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        protected override ViewResult View(string viewName, string masterName, object model)
        {
            var vmb = model as DcmsMobile.PieceReplenish.ViewModels.ViewModelBase;
            var state = (bool?)this.TempData["PieceReplenish_ModelErrors"];
            if (vmb != null)
            {
                vmb.Sound = (state ?? true) ? 'S' : 'E';
            }
            return base.View(viewName, masterName, model);
        }

        /// <summary>
        /// Asks for the carton
        /// </summary>
        /// <returns></returns>
        [Route(RestockController.ActionNameConstants.Carton, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Restock)]
        public virtual ActionResult Carton(string lastCartonId)
        {
            var model = new CartonViewModel
            {
                CartonId = lastCartonId
            };
            return View(Views.Carton, model);
        }

        [Route("UPC1", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Restock1)]
        public virtual ActionResult UPC1(string id)
        {
            
            if (string.IsNullOrWhiteSpace(id))
            {
                this.ModelState.AddModelError("", "Please scan carton again");
                return RedirectToAction(this.Actions.Carton());
            }
            return UPC(id,null,null,null);
        }
        /// <summary>
        /// This function retrieves SKU details of the passed carton, location suggestions to restock carton and then returns UPC view. 
        /// </summary>
        /// <param name="cartonId">string</param>
        /// <param name="lastUpcCode">This is passed by HandleUpc() when the user scans a UPC which is not in the carton</param>
        /// <param name="ignoreCache">Whether the passed carton can be looked up in the cache. When the user has just scanned a carton, 
        /// we do not want to use the cache because he may have recently updated the carton. Default true.</param>
        /// <returns>UPC page</returns>
        [HttpGet]
        public virtual ActionResult UPC(string cartonId, string lastUpcCode, bool? ignoreCache, string lastCartonId)
        {
            //TC1 : Press blank enter
            if (string.IsNullOrWhiteSpace(cartonId))
            {
                this.ModelState.AddModelError("", "Please scan carton again");
                return RedirectToAction(this.Actions.Carton());
            }
            cartonId = cartonId.Trim().ToUpper();
            var carton = _service.GetCartonDetails(cartonId, ignoreCache ?? true);


            //TC2 : scan wrong carton
            if (carton == null)
            {
                this.ModelState.AddModelError("", string.Format("No such carton {0}.", cartonId));
                return RedirectToAction(this.Actions.Carton());
            }

            if (carton.IsWorkNeeded && string.IsNullOrEmpty(lastCartonId))
            {
                this.AddStatusMessage("Carton require rework. Please scan carton again to proceed restock");
                return RedirectToAction(this.Actions.Carton(cartonId));
            }
            else if (lastCartonId != cartonId && !string.IsNullOrEmpty(lastCartonId))
            {
                AddStatusMessage(string.Format("New carton {0} scanned.Ignoring last Carton {1}.", cartonId, lastCartonId));
                //return RedirectToAction(this.Actions.UPC(cartonId,null,false,null));
            }
            //TC3 : Validate carton
            if (!TryValidateModel(carton))
            {
                ModelState.AddModelError("", string.Format("Ignoring Carton {0}", cartonId));
                return RedirectToAction(this.Actions.Carton());
            }
            //Populating SkuViewModel details
            var uvm = new UpcViewModel
            {
                CartonId = cartonId,
                VwhId = carton.VwhId,
                Pieces = carton.PiecesInCarton,
                SkuDisplayName = string.Format(carton.Style + "," + carton.Color + "," + carton.Dimension + "," + carton.SkuSize),
                SkuRetailPrice = carton.RetailPrice,
                LastUpcCode = lastUpcCode
            };
            uvm.SuggestedLocations = carton.AssignedLocations.Select(p => new LocationModel(p));
            return View(Views.Upc, uvm);
        }

        /// <summary>
        /// This function handles UPC scan and if UPC is valid redirects to Location Page.
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="lastUpcCode"></param>
        /// <param name="upcCode"></param>
        /// <returns>For a valid UPC redirects to Location Page else redirects to UPC action method for displaying UPC view.On null/empty scan redirects to Carton View.</returns>
        /// <remarks>
        /// This function do the following validations on the UPC:-
        /// 1) If UPC is null or empty then this function redirects to carton page.
        /// 2) If valid UPC is scanned then this function will get location suggestions to restock carton and redirects to Location page.
        /// 3) If scanned UPC is not in the scanned carton then the carton will be marked in suspense.        
        /// </remarks>
        [HttpPost]
        public virtual ActionResult HandleUpc(string cartonId, string upcCode, string lastUpcCode)
        {
            //TC4 : Press blank enter
            if (string.IsNullOrEmpty(cartonId))
            {
                ModelState.AddModelError("", "Please scan carton again");
                return RedirectToAction(Actions.Carton());
            }
            cartonId = cartonId.Trim().ToUpper();
            //TC5 : Start again
            if (string.IsNullOrWhiteSpace(upcCode))
            {
                AddStatusMessage(string.Format("Last scanned carton {0}", cartonId));
                return RedirectToAction(this.Actions.Carton());
            }
            upcCode = upcCode.Trim().ToUpper();
            var carton = _service.GetCartonDetails(cartonId, false);
            //TC6 : Scan invalid carton
            if (carton == null)
            {
                ModelState.AddModelError("", "Invalid Carton " + cartonId);
                return RedirectToAction(Actions.Carton());
            }
            //TC7 : Validate carton
            if (!TryValidateModel(carton))
            {
                ModelState.AddModelError("", string.Format("Ignoring Carton {0}", cartonId));
                return RedirectToAction(this.Actions.Carton());
            }
            var scannedSkuId = _service.GetSkuId(upcCode);
            if (scannedSkuId == null)
            {
                // The UPC scanned was not valid. Did the user scan a carton?
                if (cartonId == upcCode)
                {
                    // The user has scanned the current carton again. Ignore it.
                    return RedirectToAction(this.Actions.UPC(upcCode, null, false, null));
                }
                var newCarton = _service.GetCartonDetails(upcCode, false);
                //TC8 : Scan new carton on the behalf of UPC
                if (newCarton != null)
                {
                    AddStatusMessage(string.Format("New carton {0} scanned.Ignoring last Carton {1}.", upcCode, cartonId));
                    return RedirectToAction(this.Actions.UPC(upcCode, null, false,null));
                }
                //TC9 : Not a valid SKU.
                this.ModelState.AddModelError("", string.Format("Unrecognized UPC {0}", upcCode));
                return RedirectToAction(this.Actions.UPC(cartonId, null, false,cartonId));
            }
            //TC10 : Checking if passed UPC does not match with UPC in carton
            if (scannedSkuId == carton.SkuId)
            {
                // Normal case
                return RedirectToAction(this.Actions.Location(cartonId, null));
            }

            //TC11 : Valid SKU bar code but this SKU is not in the carton
            if (upcCode == lastUpcCode)
            {
                //TC12 : User is confirming that this is in fact the UPC in the carton
                _service.SuspenseCarton(carton);
                // TODO Show style/color etc of carton SKU and scanned SKU
                this.AddStatusMessage(string.Format("Carton {0} has been put in suspense beacause it has different SKU", cartonId));
                return RedirectToAction(this.Actions.Carton());
            }
            if (string.IsNullOrWhiteSpace(lastUpcCode))
            {
                //TC13 : The user has not been given an opportunity to confirm the UPC scan
                this.AddStatusMessage(string.Format("Carton {0} contains a different SKU? Rescan UPC to confirm and put the carton in suspense", cartonId));
                return RedirectToAction(this.Actions.UPC(cartonId, upcCode, false,null));
            }
            //TC14 : User does not want to confirm the UPC scan. Ignore this UPC.
            this.AddStatusMessage(string.Format("UPC Scan was not confirmed.Carton {0} contains a different SKU. Rescan {1} UPC to put the carton in suspense", cartonId, upcCode));
            return RedirectToAction(this.Actions.UPC(cartonId, upcCode, false,null));
        }

        /// <summary>
        /// This function retrieves suggestions of locations to restock the scanned carton.
        /// </summary>
        /// <param name="cartonId">string</param>
        /// <param name="isConfirm">bool</param>
        /// <returns>Location Page</returns>
        [HttpGet]
        public virtual ActionResult Location(string cartonId, string lastLocationId)
        {
            if (string.IsNullOrWhiteSpace(cartonId))
            {
                this.ModelState.AddModelError("", "Please scan carton");
                return RedirectToAction(this.Actions.Carton());
            }
            cartonId = cartonId.Trim().ToUpper();
            //Fetching details of carton
            var carton = _service.GetCartonDetails(cartonId, false);
            if (carton == null)
            {
                this.ModelState.AddModelError("", string.Format("Invalid Carton {0}", cartonId));
                return RedirectToAction(this.Actions.Carton());
            }
            if (!TryValidateModel(carton))
            {
                ModelState.AddModelError("", string.Format("Ignoring Carton {0}", cartonId));
                return RedirectToAction(this.Actions.Carton());
            }
            //Fetching suggestions of location for carton to restock
            var lvm = new LocationViewModel
           {
               SuggestedLocations = carton.AssignedLocations.Select(p => new LocationModel(p)),
               CartonId = carton.CartonId,
               SkuDisplayName = string.Format(carton.Style + "," + carton.Color + "," + carton.Dimension + "," + carton.SkuSize),
               SkuRetailPrice = carton.RetailPrice,
               VwhId = carton.VwhId,
               LastLocationId = lastLocationId,
               PiecesInCarton = carton.PiecesInCarton
           };
            return View(Views.Location, lvm);
        }

        /// <summary>
        /// This function restocks carton on the valid scanned location
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="isConfirm"></param>
        /// <param name="locationId"></param>
        /// <returns>For valid Location scan restocks carton else redirects to Location action method displaying Location view.For null/empty scan redirects to UPC view</returns>
        /// <remarks>
        /// This function do the following validations on the Location:-
        /// 1) If location is null or empty then this function redirects to UPC page.
        /// 2) This function gets scanned carton and scanned location details and validate if the scanned location have carton's SKU assigned to it or not.
        /// 3) This function checks for scanned locations SKU demand is more or less than pieces of SKU on carton.If carton contains more pieces than pieces demanded by location we ask for a confirmation scan.
        /// </remarks>
        [HttpPost]
        public virtual ActionResult RestockCarton(string cartonId, string locationId, string lastLocationId)
        {
            if (string.IsNullOrEmpty(cartonId))
            {
                ModelState.AddModelError("", "Please scan carton again");
                return RedirectToAction(Actions.Carton());
            }
            cartonId = cartonId.Trim().ToUpper();
            //Checking null scan
            if (string.IsNullOrWhiteSpace(locationId))
            {
                AddStatusMessage(string.Format("Last scanned carton {0}", cartonId));
                return RedirectToAction(this.Actions.Carton());
            }
            locationId = locationId.Trim().ToUpper();
            //Fetching carton details
            var carton = _service.GetCartonDetails(cartonId, false);
            if (carton == null)
            {
                this.ModelState.AddModelError("", string.Format("Invalid Carton {0}", cartonId));
                return RedirectToAction(this.Actions.Carton());
            }
            var location = carton.AssignedLocations.Where(p => p.LocationId == locationId).FirstOrDefault();
            //TC15 : Invalid location
            if (location == null)
            {
                this.ModelState.AddModelError("", string.Format("The SKU in Carton {0} is not assigned to Location {1}", cartonId, locationId));
                return RedirectToAction(this.Actions.Location(cartonId, null));               
            }

            //TC16 : Checking if pieces in carton are more than pieces required by location
            if (carton.PiecesInCarton <= location.SpaceAvailable || locationId == lastLocationId)
            {
                carton.RestockAtLocation = locationId;
                // Sharad 2 Nov 2013: Need to capture building in productivity
                carton.BuildingId = location.BuildingId;
                carton.PickAreaId = location.IaId;
                try
                {
                    _service.RestockCarton(carton, locationId);
                    this.AddStatusMessage(string.Format("Carton {0} restocked at location {1}", cartonId, locationId));
                }
                catch (DbException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            else
            {
                this.AddStatusMessage("Carton contains more pieces then required by location. If you still want to restock carton at this location please scan this location again or scan a new location.");
                return RedirectToAction(this.Actions.Location(cartonId, locationId));
            }

            return RedirectToAction(this.Actions.Carton());
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
                        AddStatusMessage("Please scan the carton again.");
                        result = RedirectToAction(this.Actions.Carton());
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
