using System;
using System.Collections.Generic;
using System.Web;


namespace DcmsMobile.BoxManager.Repository.VasConfiguration
{
    public class VasConfigurationService : IDisposable
    {

        #region Intialization

        private readonly VasConfigurationRepository _repos;

        public VasConfigurationService(VasConfigurationRepository repos)
        {
            _repos = repos;
        }

        /// <summary>
        /// Used to store destination area of intransit cartons until they are received
        /// </summary>      
        public VasConfigurationService(TraceContext ctx, string connectString, string userName, string clientInfo, string moduleName)
        {
            _repos = new VasConfigurationRepository(ctx, connectString, userName, clientInfo, moduleName);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }

        #endregion

        /// <summary>
        /// Returns the list of all VAS configurations
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescription> GetVasList()
        {
            return _repos.GetVasList();
        }

        /// <summary>
        /// Returns the list of VAS settings
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CustomerVasSetting> GetCustomerVasSettings(string customerId)
        {
            return _repos.GetCustomerVasSettings(customerId);
        }

        /// <summary>
        /// Adds new VAS configuration with default settings
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vasId"></param>
        public void AddVasConfiguration(string customerId, string vasId)
        {
            _repos.UpdateVasConfiguration(customerId, vasId, ".*@.*", "All Labels", "", true, true);
        }

        /// <summary>
        /// Updates the remarks and inactive flag of VAS Configuration.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vasId"></param>
        /// <param name="remarks"></param>
        /// <param name="inactiveFlag"></param>
        public void UpdateConfigurationRemark(string customerId, string vasId, string remarks, bool inactiveFlag)
        {
            _repos.UpdateVasConfiguration(customerId, vasId, null, null, remarks, inactiveFlag, false);
        }

        /// <summary>
        /// Try to add VAS setting for passed customer. If setting is already added then update it
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vasId"></param>
        /// <param name="description"> </param>
        /// <param name="userRemarks"> </param>
        /// <param name="inactiveFlag"> </param>
        /// <param name="pattern"> </param>
        /// <returns></returns>
        public void UpdateVasConfiguration(string customerId, string vasId, string pattern, string description, string userRemarks, bool inactiveFlag)
        {
            _repos.UpdateVasConfiguration(customerId, vasId, pattern, description, userRemarks, inactiveFlag, true);
        }

        /// <summary>
        /// Removes the passed customer <see cref="customerId"/> VAS pattern of specific VAS type <see cref="vasId"/>
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vasId"></param>
        /// <returns></returns>
        public bool RemoveVasConfiguration(string customerId, string vasId)
        {
            return _repos.RemoveVasConfiguration(customerId, vasId);
        }

        /// <summary>
        /// Returns the comprehensive list of POs, which qualify the PO REGEX pattern + Label REGEX pattern, Including the customer and Vas Id
        /// </summary>
        /// <param name="vasId"> </param>
        /// <param name="regExpOld"></param>
        /// <param name="regExpNew"></param>
        /// <param name="customerId"> </param>
        /// <returns></returns>
        public IEnumerable<Tuple<VasConfigurationRepository.PoQualificationType, string>> GetComprehensivePoList(string customerId, string vasId, string regExpOld, string regExpNew)
        {
            return _repos.GetComprehensivePoList(customerId, vasId,regExpOld, regExpNew);
        }

        /// <summary>
        /// Returns the list of POs for passed customer, which qualify the VAS REGEX including PO/Label pattern
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vasId"></param>
        /// <param name="vasRegExp"></param>
        /// <returns></returns>
        public IEnumerable<string> GetQualifyingCustomerPos(string customerId, string vasId, string vasRegExp)
        {
            return _repos.GetQualifyingCustomerPos(customerId, vasId, vasRegExp);
        }

        /// <summary>
        /// This method disables the VAS configuration for passed customer and VAS, on basis of all/selective non-validated orders .
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vasId"></param>
        /// <param name="regExPattern"> </param>
        /// <param name="currentOrdersOnly">
        /// True: method will disable configuration from Current orders only.
        /// False: method will disable configuration from All orders excluding Current orders.
        /// Null: method will disable configuration from All orders.
        /// </param>
        internal void DisableVasConfiguration(string customerId, string vasId, string regExPattern, bool? currentOrdersOnly)
        {
            _repos.DisableVasConfiguration(customerId, vasId, regExPattern, currentOrdersOnly);
        }

        /// <summary>
        /// This method enables the VAS configuration for passed customer and VAS, on the basis of all/selective non-validated orders.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vasId"></param>
        /// <param name="regExPattern"> </param>
        /// <param name="allOrders">
        /// True: method will enable configuration from All orders.
        /// False: method will enable configuration from All orders excluding Current orders.
        /// </param>
        internal void EnableVasConfiguration(string customerId, string vasId, string regExPattern, bool allOrders)
        {
            _repos.EnableVasConfiguration(customerId, vasId, regExPattern, allOrders);
        }
    }
}