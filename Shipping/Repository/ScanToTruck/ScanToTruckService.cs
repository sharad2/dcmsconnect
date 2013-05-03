using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.Shipping.Repository.ScanToTruck
{
    //Reviewed By: Ravneet, Binod and Deepak 15 Dec 2012
    public enum ScanToTruckServiceErrorCode
    {
        Unknown,

        /// <summary>
        /// When scanned pallet does not have any box.
        /// </summary>
        InvalidPallet,

        /// <summary>
        /// When pallet have unverified boxes.
        /// </summary>
        UnVerifiedBoxes,

        /// <summary>
        /// When pallet have boxes which are Cancelled.
        /// </summary>
        StopProcess,

        /// <summary>
        /// If the pallet has already been loaded on truck
        /// </summary>
        LoadedOnTruck,

        /// <summary>
        /// If the appointment number are mismatch
        /// </summary>
        AppointmentMisMatch
    }

    /// <summary>
    /// Sounds to confirm scans. Used by home controller.
    /// </summary>
    public enum Sound
    {
        /// <summary>
        /// Error Sound when something catastrophic happens
        /// </summary>
        Error = 'E',

        /// <summary>
        /// When something unusual happens
        /// </summary>
        Warning = 'W',

        /// <summary>
        /// Nothing happened.
        /// </summary>
        None = '\0'
    }

    /// <summary>
    /// This class provides the handling of ScanToTruck program exceptions.
    /// </summary>
    internal sealed class ScanToTruckServiceException : Exception
    {
        public ScanToTruckServiceException()
        {
        }

        private readonly ScanToTruckServiceErrorCode _errorCode;
        public ScanToTruckServiceException(ScanToTruckServiceErrorCode errorCode)
        {
            _errorCode = errorCode;
        }

        public ScanToTruckServiceException(ScanToTruckServiceErrorCode errorCode, string diagnostic)
        {
            _errorCode = errorCode;
            this.Data.Add("Data", diagnostic);
        }

        public ScanToTruckServiceErrorCode ErrorCode
        {
            get
            {
                return _errorCode;
            }
        }
    }

    internal class ScanToTruckService : IDisposable
    {
        #region Intialization

        private readonly ScanToTruckRepository _repos;

        public ScanToTruckService(TraceContext ctx, string connectString, string userName, string clientInfo)
        {
#if DEBUG
            if (userName.StartsWith("_"))
            {
                // This is a dummy user. Don't let the repository know about this
                userName = "";
            }
#endif
            _repos = new ScanToTruckRepository(ctx, connectString, userName, clientInfo);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }


        #endregion

        /// <summary>
        /// This variable use for local caching
        /// </summary>
        private Dictionary<string, IEnumerable<Box>> _boxes;

        /// <summary>
        /// Get all pallets of the passed appointment.
        /// </summary>
        /// <param name="appointmentNum"></param>
        /// <returns></returns>
        internal ICollection<Pallet> GetPalletSuggestion(int appointmentNum)
        {
            return _repos.GetSuggestedPallet(appointmentNum);
        }

        /// <summary>
        /// Try to load passed pallet.
        /// </summary>
        /// <param name="palletId"></param>
        internal void LoadPallet(string palletId)
        {
            // Validate Pallet for given appointment.
            this.ValidatePallet(palletId);
            //Update pallet.
            _repos.LoadPallet(palletId);
        }

        /// <summary>
        /// Ensure that passed boxes are satisfied all condition as verified etc...
        /// </summary>
        /// <param name="palletId"> </param>
        private void ValidatePallet(string palletId)
        {
            // Get boxes of passed pallet for validate pallet.
            var boxes = GetBoxesOfPallet(palletId);

            if (boxes != null && !boxes.Any())
            {
                throw new ScanToTruckServiceException(ScanToTruckServiceErrorCode.InvalidPallet);
            }

            var truckLoadBoxCount = boxes.Count(p => p.TruckLoadDate != null);
            if (truckLoadBoxCount > 0)
            {
                throw new ScanToTruckServiceException(ScanToTruckServiceErrorCode.LoadedOnTruck, truckLoadBoxCount.ToString());
            }
            var unVerifiedBoxCount = boxes.Count(p => p.VerifyDate == null);
            var stopProcessBoxCount = boxes.Count(p => p.StopProcessDate != null);
            if (unVerifiedBoxCount > 0)
            {
                throw new ScanToTruckServiceException(ScanToTruckServiceErrorCode.UnVerifiedBoxes, unVerifiedBoxCount.ToString());
            }
            if (stopProcessBoxCount > 0)
            {
                throw new ScanToTruckServiceException(ScanToTruckServiceErrorCode.StopProcess, stopProcessBoxCount.ToString());
            }
        }

        /// <summary>
        /// Get all boxes of passed pallet.
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>
        internal IEnumerable<Box> GetBoxesOfPallet(string palletId)
        {
            //Creating local cache here
            if (_boxes == null || _boxes[palletId] == null)
            {
                _boxes = new Dictionary<string, IEnumerable<Box>>();
                _boxes[palletId] = _repos.GetBoxesOfPallet(palletId);
            }
            return _boxes[palletId];
        }

        /// <summary>
        /// Unload passed pallet.
        /// </summary>
        /// <param name="palletId"></param>
        internal void UnLoadPallet(string palletId)
        {
            _repos.UnLoadPallet(palletId);
        }

        /// <summary>
        /// Get appointment summery information as total pallet, loaded pallet, unpalletize boxes etc..
        /// </summary>
        /// <param name="appointmentNo"></param>
        /// <returns></returns>
        internal Appointment GetAppointmentInfo(int appointmentNo)
        {
            return _repos.GetAppointmentDetails(appointmentNo);
        }

    }
}