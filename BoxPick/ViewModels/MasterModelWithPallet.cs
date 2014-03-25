using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DcmsMobile.BoxPick.Models;

namespace DcmsMobile.BoxPick.ViewModels
{
    /// <summary>
    /// Pick mode type will be used to decide whether operation will
    /// be performed for BOX PICKING or ADR PULLING
    /// </summary>
    public enum PickModeType
    {
        ADR,
        ADREPPWSS
    }

    /// <summary>
    /// Pallet required. Key information about what needs to be picked is also required. All information is stored in session state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A minimally valid pallet will look like this:
    /// </para>
    /// <code>
    /// <![CDATA[
    ///var pallet = new Pallet
    ///{
    ///    PalletId = "P12345",
    ///    QueryTime = DateTime.Now,
    ///    BoxToPick = new Box
    ///    {
    ///        UccId = "00001234567890123456",
    ///        SkuInBox = new Sku
    ///        {
    ///            SkuId = 123
    ///        },
    ///        Pieces = 6,
    ///        QualityCode = "01",
    ///        VwhId = "C15"
    ///    },
    ///    CartonSourceArea = "BIR"
    ///    TotalBoxCount = 5,
    ///    PickedBoxCount =2
    ///};
    /// ]]>
    /// </code>
    /// </remarks>
    [ModelBinder(typeof(MasterModelBinder))]
    public class MasterModelWithPallet : MasterModel
    {
        private static PickModeType? StringToPickModeType(string pickMode)
        {
            switch (pickMode)
            {
                //For the bucket's pick mode PITCHING and ADRE we will return the pick mode as ADR.
                //ADR pick mode is for the buckets against which post printing is being done.
                //ADREPPWSS pick mode is for the buckets agasint whcih pre-printing is being done.
                case "PITCHING":
                case "ADRE":
                    return PickModeType.ADR;

                case "ADREPPWSS":
                    return PickModeType.ADREPPWSS;

                default:
                    return (PickModeType?)null;
            }
        }

        private class PalletSessionData
        {
#pragma warning disable 649
            public DateTime? QueryTime;
            public string PalletId;
            public int? SkuIdToPick;
            public string SkuDisplayNameToPick;
            public int? PiecesToPick;
            public string QualityCodeToPick;
            public string VwhIdToPick;
            public string UccIdToPick;
            public int PickedBoxCount;
            public int PickableBoxCount;
            public int TotalBoxCount;
            public string CartonIdToPick;
            public CartonLocation[] CartonLocations;
            public string CurrentLocationId;
            public PickModeType? PickMode;
            public int CountRequiredVAS;
            public string AssociatedCartonPalletId { get; set; }
#pragma warning restore 649

        }

        private PalletSessionData _sessionData;

        /// <summary>
        /// The key against which info is stored in session. Public for use by unit testing
        /// </summary>
        private const string SESSION_KEY_MASTERMODELWITHPALLET_SESSIONDATA = "MasterModelWithPallet";

        public MasterModelWithPallet(HttpSessionStateBase session)
            : base(session)
        {
            _sessionData = _session[SESSION_KEY_MASTERMODELWITHPALLET_SESSIONDATA] as PalletSessionData;
            if (_sessionData == null)
            {
                _sessionData = new PalletSessionData();
                _session[SESSION_KEY_MASTERMODELWITHPALLET_SESSIONDATA] = _sessionData;
            }
        }

        /// <summary>
        /// The pallet which is being picked. If this is null, then all pallet properties return null
        /// </summary>
        [Required(ErrorMessage = "Pallet is required")]
        public string CurrentPalletId
        {
            get
            {
                return this._sessionData.PalletId;
            }
        }

        /// <summary>
        /// The time at which productivity clock starts ticking.
        /// </summary>
        /// <remarks>
        /// It is set when a new carton is proposed. This means:
        /// 1. New pallet accepted
        /// 2. Carton picked.
        /// 3. UCC Skipped.
        /// 
        /// Required because we need it at the time box is picked
        /// </remarks>
        [Required]
        public DateTime? ProductivityStartTime
        {
            get
            {
                return this._sessionData.QueryTime;
            }
        }

        /// <summary>
        /// The SKU Id within the box to be picked.
        /// </summary>
        /// <remarks>
        /// Required to match with SKU in scanned carton
        /// </remarks>
        [Required(ErrorMessage = "The box must contain an SKU.")]
        public int? SkuIdToPick
        {
            get
            {
                return this._sessionData.SkuIdToPick;
            }
        }

        /// <summary>
        /// Friendly way to display the SKU to pick
        /// </summary>
        [DisplayFormat(NullDisplayText = "Empty SKU")]
        [Display(Name = "SKU")]
        public string SkuDisplayNameToPick
        {
            get
            {
                return this._sessionData.SkuDisplayNameToPick;
            }
        }

        /// <summary>
        /// Number of pieces within the box to pick
        /// </summary>
        /// <remarks>
        /// Required to match with pieces in scanned carton
        /// </remarks>
        [Required(ErrorMessage = "The box must not be empty.")]
        [Display(Name = "Pcs")]
        public int? PiecesToPick
        {
            get
            {
                return this._sessionData.PiecesToPick;
            }
        }

        /// <summary>
        /// Quality code of the box to pick
        /// </summary>
        /// <remarks>
        /// Required to match with quality code in scanned carton
        /// </remarks>
        [Required(ErrorMessage = "The box must have quality code assigned")]
        [Display(Name = "Quality")]
        public string QualityCodeToPick
        {
            get
            {
                return this._sessionData.QualityCodeToPick;
            }
        }

        /// <summary>
        /// Virtual warehouse of the box to pick
        /// </summary>
        /// <remarks>
        /// Required to match with Virtual warehouse in scanned carton
        /// </remarks>
        [Required(ErrorMessage = "The box must have virtual warehouse assigned.")]
        [Display(ShortName = "Vwh", Name = "Virtual Warehouse")]
        public string VwhIdToPick
        {
            get
            {
                return this._sessionData.VwhIdToPick;
            }
        }


        //[Required(ErrorMessage = "Pull carton area not specified for the bucket to which this pallet belongs")]
        //[Display(Name = " ")]
        //public string CartonSourceAreaToPick
        //{
        //    get
        //    {
        //        return this._sessionData.CartonSourceArea;
        //    }
        //}

        /// <summary>
        /// UccId of the label for which a carton is being picked
        /// </summary>
        ///<remarks>
        /// Required to pass to pick carton.
        /// </remarks>
        [Required(ErrorMessage = "Pallet must have a box to pick.")]
        [Display(Name = "Box")]
        public string UccIdToPick
        {
            get
            {
                return this._sessionData.UccIdToPick;
            }
        }

        /// <summary>
        /// The number of boxes on this pallet which have already been picked
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PalletPickedBoxCount
        {
            get
            {
                return this._sessionData.PickedBoxCount;
            }
        }

        /// <summary>
        /// The number of boxes remaining to be picked for this pallet
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PalletPickableBoxCount
        {
            get
            {
                return this._sessionData.PickableBoxCount;
            }
        }

        /// <summary>
        /// Number of boxes the pallet will have after it has been fully picked
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PalletTotalBoxCount
        {
            get
            {
                return this._sessionData.TotalBoxCount;
            }
        }

        /// <summary>
        /// Displayed by the view
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:p0}")]
        public double? PercentComplete
        {
            get
            {
                if (PalletTotalBoxCount == 0)
                {
                    return null;
                }
                return Math.Round((double)PalletPickedBoxCount / (double)PalletTotalBoxCount * 100, 2);
            }
        }

        public int CountRequiredVAS
        {
            get
            {
                return this._sessionData.CountRequiredVAS;
            }
        }
        /// <summary>
        /// The carton id which was reserved for the current box
        /// </summary>
        [DisplayFormat(NullDisplayText = "Pick any carton of required SKU, Pieces and Quality")]
        [Display(Name = "Suggested Carton")]
        public string CartonIdToPick
        {
            get
            {
                return this._sessionData.CartonIdToPick;
            }
        }

        /// <summary>
        /// Pick mode is set in the map function
        /// </summary>
        /// <remarks>
        /// <para>
        /// PickMode will be null if the bucket has a pick mode which we do not recognize.
        /// TODO: Give a better diagnostic.
        /// </para>
        /// </remarks>
        [Required(ErrorMessage = "Box Picking not supported for this pallet")]
        public PickModeType? PickMode
        {
            get
            {
                return this._sessionData.PickMode;
            }
        }

        /// <summary>
        /// Pass a null pallet to clear the current pallet
        /// </summary>
        /// <param name="pallet"></param>
        public void Map(Pallet pallet)
        {
            if (pallet == null)
            {
                _sessionData = new PalletSessionData();
                _session[SESSION_KEY_MASTERMODELWITHPALLET_SESSIONDATA] = _sessionData;
                return;
            }
            _sessionData.SkuIdToPick = pallet.BoxToPick.SkuInBox.SkuId;
            _sessionData.SkuDisplayNameToPick = pallet.BoxToPick.SkuInBox.DisplayName;
            _sessionData.PiecesToPick = pallet.BoxToPick.Pieces;
            _sessionData.QualityCodeToPick = pallet.BoxToPick.QualityCode;
            _sessionData.VwhIdToPick = pallet.BoxToPick.VwhId;
            _sessionData.UccIdToPick = pallet.BoxToPick.UccId;
            _sessionData.CartonIdToPick = pallet.BoxToPick.AssociatedCarton.CartonId;
            _sessionData.CurrentLocationId = pallet.BoxToPick.AssociatedCarton.LocationId;
            _sessionData.PickMode = StringToPickModeType(pallet.PickModeText);
            _sessionData.PalletId = pallet.PalletId;
            _sessionData.CartonLocations = pallet.CartonLocations;
            _sessionData.PickableBoxCount = pallet.PickableBoxCount;
            _sessionData.PickedBoxCount = pallet.PickedBoxCount;
            _sessionData.QueryTime = pallet.QueryTime;
            _sessionData.TotalBoxCount = pallet.TotalBoxCount;
            _sessionData.CountRequiredVAS = pallet.CountRequiredVAS;
            _sessionData.AssociatedCartonPalletId = pallet.BoxToPick.AssociatedCarton.AssociatedPalletId;

        }

        /// <summary>
        /// To Show number of cartons to be picked from the next locations and locations,
        /// currently it shows data for only subsequent 6 locations
        /// </summary>
        public IEnumerable<string> CartonLocations
        {
            get
            {
                return this._sessionData.CartonLocations == null ? Enumerable.Empty<string>() : this._sessionData.CartonLocations.Select(p => p.CartonLocationId);
            }
        }

        [DisplayFormat(NullDisplayText = "Unknown")]
        [Display(Name = "Location", ShortName = "Loc")]
        public string CurrentLocationId
        {
            get
            {
                return this._sessionData.CurrentLocationId;
            }
        }

        /// <summary>
        /// Dispalys current location. If current location is null then it displays carton pallet. 
        /// </summary>
        [Display(Name = "Location", ShortName = "Loc")]
        public string DisplayLocationId
        {

            get
            {
                if (string.IsNullOrEmpty(this._sessionData.CurrentLocationId))
                {
                    return this._sessionData.AssociatedCartonPalletId;
                }
                else
                {

                    return this._sessionData.CurrentLocationId;
                }
            }

        }

        [Display(Name = "Cartons to pick", ShortName = "# Ctn")]
        [DisplayFormat(NullDisplayText = "Unknown")]
        public int? CountCartonsToPickFromLocation
        {
            get
            {
                if (this.CurrentLocationId != null || this._sessionData.CartonLocations != null)
                {
                    return this._sessionData.CartonLocations.Where(p => p.CartonLocationId == this.CurrentLocationId && p.SkuToPick.SkuId == this.SkuIdToPick).Select(p => p.CountCartonsToPick).FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }

        public IEnumerable<int?> CartonCountAtLocations
        {
            get
            {
                return this._sessionData.CartonLocations == null ? Enumerable.Empty<int?>() : this._sessionData.CartonLocations.Select(p => p.CountCartonsToPick)
                    .Cast<int?>();
            }
        }

    }
}



//$Id$