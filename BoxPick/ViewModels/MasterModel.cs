using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace DcmsMobile.BoxPick.ViewModels
{
    /// <summary>
    /// Designed for master page. No validations here
    /// </summary>
    /// <remarks>
    /// The model can only be created by passing session to the constructor or via model binding.
    /// </remarks>
    [ModelBinder(typeof(MasterModelBinder))]
    public class MasterModel
    {
        #region Session management
        /// <summary>
        /// This class is public so that unit tests can create an instance and store in session
        /// </summary>
        private class SessionData
        {
            public string LastUccId;
            public string LastCartonId;
            public string LastLocation;

            /// <summary>
            /// Whether the current carton has been picked. Becomes true after the UCC has been scanned. For ADREPPWSS, becomes true after UCC scan. For ADRE, never becomes true.
            /// </summary>
            public bool IsPicked;
            public string CurrentBuildingId;
            public string CurrentDestinationArea;            
            public string CurrentDestAreaShortName;
            public string CurrentSourceArea;
            public string CurrentSourceAreaShortName;
        }

        /// <summary>
        /// The key against which info is stored in session. Public for use by unit testing
        /// </summary>
        private const string SESSION_KEY_MASTERMODEL_SESSIONDATA = "MasterModel";

        protected readonly HttpSessionStateBase _session;

        private SessionData SessionValues
        {
            get
            {
                var _sessionData = _session[SESSION_KEY_MASTERMODEL_SESSIONDATA] as SessionData;
                if (_sessionData == null)
                {
                    _sessionData = new SessionData();
                    _session[SESSION_KEY_MASTERMODEL_SESSIONDATA] = _sessionData;
                }
                return _sessionData;
            }
        }

        /// <summary>
        /// Use this to access session saved properties.
        /// </summary>
        /// <param name="session"></param>
        /// <remarks>
        /// Useful when you do not intend to bind the model
        /// </remarks>
        public MasterModel(HttpSessionStateBase session)
        {
            Contract.Requires(session != null);
            _session = session;

        }

        /// <summary>
        /// Clearing just box pick portion of the session, and not the entire session. Clearing entire session is bad and it breaks mobile emulation.
        /// </summary>
        /// <param name="session"></param>
        internal static void ClearSessionValues(HttpSessionStateBase session)
        {
            session[SESSION_KEY_MASTERMODEL_SESSIONDATA] = null;
        }

        protected ActionResult _mainContentAction;

        /// <summary>
        /// The child action to call which will render the main content in desk top views
        /// </summary>
        public virtual ActionResult MainContentAction
        {
            get
            {
                return _mainContentAction;
            }
        }
        #endregion

        /// <summary>
        /// Sound file to play on page load.
        /// </summary>
        public char Sound
        {
            get;
            set;
        }

        /// <summary>
        /// Whether the current carton has been picked. Becomes true after the UCC has been scanned. For ADREPPWSS, becomes true after UCC scan. For ADRE, never becomes true.
        /// </summary>
        public bool IsPicked
        {
            get
            {
                return this.SessionValues.IsPicked;
            }
        }

        /// <summary>
        /// The building becomes current only after successful validation
        /// </summary>
        [Display(Name = "Current Building", ShortName = "Building")]
        [DisplayFormat(NullDisplayText = "Bldg")]
        public virtual string CurrentBuildingId
        {
            get
            {
                return this.SessionValues.CurrentBuildingId;
            }
            set
            {
                this.SessionValues.CurrentBuildingId = value;
            }
        }

        /// <summary>
        /// The destination area for which boxes are being pulled. For example, ADR
        /// </summary>
        [Display(Name = "Current Destination Area", ShortName = "Area")]
        [DisplayFormat(NullDisplayText = "Any Area")]
        public string CurrentDestinationArea
        {
            get
            {
                return this.SessionValues.CurrentDestinationArea;
            }
            set
            {
                this.SessionValues.CurrentDestinationArea = value;
            }
        }

        public string CurrentDestAreaShortName
        {
            get
            {
                return this.SessionValues.CurrentDestAreaShortName;
            }
            set
            {
                this.SessionValues.CurrentDestAreaShortName = value;
            }
        }


        /// <summary>
        /// The area in which the carton to pick will be found
        /// </summary>
        /// <remarks>
        /// Required to match with Storage Area in scanned carton
        /// </remarks>
        [Display(Name = "Current Source Area", ShortName = "Src Area")]
        [DisplayFormat(NullDisplayText = " ")]
        public string CurrentSourceArea
        {
            get
            {
                return this.SessionValues.CurrentSourceArea;
            }
            set
            {
                this.SessionValues.CurrentSourceArea = value;
            }
        }

        public string CurrentSourceAreaShortName
        {
            get
            {
                return this.SessionValues.CurrentSourceAreaShortName;
            }
            set
            {
                this.SessionValues.CurrentSourceAreaShortName = value;
            }
        }

        /// <summary>
        /// When a carton is scanned, captures the scanned carton and its location
        /// </summary>
        /// <param name="lastCartonId"></param>
        /// <param name="locationId"></param>
        internal void SetLastCartonAndLocation(string lastCartonId, string locationId)
        {
            this.SessionValues.LastCartonId = lastCartonId;
            this.SessionValues.LastLocation = locationId;
            this.SessionValues.LastUccId = null;
            this.SessionValues.IsPicked = false;
        }

        /// <summary>
        /// Captures the UCC scanned and completes the picking process for the carton.
        /// </summary>
        /// <param name="uccId"></param>
        internal void SetLastUccPicked(string uccId)
        {
            this.SessionValues.LastUccId = uccId;
            this.SessionValues.IsPicked = true;
        }

        /// <summary>
        /// The carton which was last successfully scanned. Displayed by view. Also relied upon when UCC is picked.
        /// This is the carton to which the UCC is affixed. Set when a valid carton is scanned.
        /// </summary>
        [Display(Name = "Carton", ShortName = "Ctn")]
        [DisplayFormat(NullDisplayText = "None")]
        public virtual string LastCartonId
        {
            get
            {
                return this.SessionValues.LastCartonId;
            }
        }

        /// <summary>
        /// Displayed by the view. Set after a UCC is scanned, successfully or not.
        /// </summary>
        [Display(Name = "Box")]
        [DisplayFormat(NullDisplayText = "None")]
        public string LastUccId
        {
            get
            {
                return this.SessionValues.LastUccId;
            }
        }

        /// <summary>
        /// Displayed by the view. Set by the controller when a carton is scanned.
        /// </summary>
        [Display(Name = "Carton Location", ShortName = "Loc")]
        [DisplayFormat(NullDisplayText = "None")]
        public string LastLocation
        {
            get
            {
                return this.SessionValues.LastLocation;
            }
        }

        /// <summary>
        /// Creates the model by passing session to the constructor.
        /// </summary>
        protected class MasterModelBinder : DefaultModelBinder
        {
            protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
            {
                var model = Activator.CreateInstance(modelType, controllerContext.HttpContext.Session);
                return model;
            }

            /// <summary>
            /// Converts value to upper case and perform trim operations on properties who has UIHint["scan"] attribute defined and is of string type.
            /// </summary>
            protected override void SetProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, object value)
            {
                var prop = bindingContext.ModelType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .Where(p => p.GetCustomAttributes(typeof(UIHintAttribute), false)
                                .Cast<UIHintAttribute>().Any(q => q.UIHint == "scan"))
                            .SingleOrDefault(p => !string.IsNullOrEmpty(controllerContext.HttpContext.Request.Form[p.Name]));

                if (prop != null && value != null && propertyDescriptor.PropertyType == typeof(string))
                {
                    string newvalue = value.ToString().Trim().ToUpperInvariant();
                    base.SetProperty(controllerContext, bindingContext, propertyDescriptor, newvalue);
                }
                else
                {
                    base.SetProperty(controllerContext, bindingContext, propertyDescriptor, value);
                }
            }
        }
    }
}



//$Id$