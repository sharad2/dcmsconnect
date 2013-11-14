using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace DcmsMobile.PickWaves.Repository.Config
{
    public class ConfigService : PickWaveServiceBase<ConfigRepository>
    {
        internal class Constraint
        {
            public Constraint()
            {

            }
            /// <summary>
            /// Generate constraints based on the passed list of SPLH
            /// </summary>
            /// <param name="splhList"></param>
            public Constraint(IEnumerable<ConfigRepository.Splh> splhList)
            {
                var constraints = new Dictionary<string, string>();

                foreach (var splh in splhList.Where(splh => !constraints.ContainsKey(splh.SplhId)))
                {
                    constraints.Add(splh.SplhId, splh.SplhValue);
                }

                string value;
                if (constraints.TryGetValue("$BOXMAXWT", out value))
                {
                    decimal result;
                    if (decimal.TryParse(value, out result))
                    {
                        this.MaxBoxWeight = (int)result;
                    }
                }
                if (constraints.TryGetValue("$MAXSKUPB", out value))
                {
                    int result;
                    if (int.TryParse(value, out result))
                    {
                        this.MaxSkuWithinBox = result;
                    }
                }

                if (constraints.TryGetValue("$SSB", out value))
                {
                    this.IsSingleStyleColor = value == "SC";
                }

                if (constraints.TryGetValue("_$MINSKUPIECES", out value))
                {
                    int result;
                    if (int.TryParse(value, out result))
                    {
                        this.RequiredMinSkuPieces = result;
                    }
                }
                if (constraints.TryGetValue("_$MAXSKUPIECES", out value))
                {
                    int result;
                    if (int.TryParse(value, out result))
                    {
                        this.RequiredMaxSkuPieces = result;
                    }
                }
            }

            /// <summary>
            /// The maximum permissible weight of the box after SKUs have been added to it.
            /// </summary>
            public int? MaxBoxWeight { get; set; }

            public int? MaxSkuWithinBox { get; set; }

            /// <summary>
            /// Required Min pieces in box of a single SKU
            /// </summary>
            public int? RequiredMinSkuPieces { get; set; }

            /// <summary>
            /// Required Max pieces in box of a single SKU
            /// </summary>
            public int? RequiredMaxSkuPieces { get; set; }

            /// <summary>
            /// Box which contains Single Style color  skus.
            /// </summary>
            public bool IsSingleStyleColor { get; set; }
        }

        #region Intialization

        public ConfigService(TraceContext trace, string userName, string clientInfo)
        {
            _repos = new ConfigRepository(trace, userName, clientInfo);
        }

        #endregion

        internal Constraint GetDefaultConstraints()
        {
            return new Constraint(_repos.GetDefaultSplhValues());
        }

        internal Constraint GetCustomerConstraints(string customerId, out string customerName)
        {
            var result = _repos.GetCustomerSplhValues(customerId);
            customerName = result.Select(p => p.CustomerName).First();
            return new Constraint(result);
        }

        /// <summary>
        /// Key is customer Id
        /// </summary>
        /// <returns></returns>
        internal IDictionary<Customer, Constraint> GetAllCustomerConstraints()
        {
            var custConstraints = _repos.GetCustomerSplhValues(null);
            var query = (from item in custConstraints
                         group item by new Customer
                         {
                             CustomerId = item.CustomerId,
                             Name = item.CustomerName
                         } into g
                         select g).ToDictionary(p => p.Key, p => new Constraint(p));
            return query;
        }

        public void UpdateMaxBoxWeight(string customerId, decimal? splhValue)
        {
            _repos.UpdateCustomerSplhValue(customerId, "$BOXMAXWT", splhValue.HasValue ? splhValue.ToString() : null);
        }

        public void UpdateMaxSkuInBox(string customerId, int? splhValue)
        {
            _repos.UpdateCustomerSplhValue(customerId, "$MAXSKUPB", splhValue.HasValue ? splhValue.ToString() : null);
        }

        internal void UpdateSkuMixing(string customerId, bool isSingleStyleColor)
        {
            _repos.UpdateCustomerSplhValue(customerId, "$SSB", isSingleStyleColor ? "SC" : null);
        }


        public void UpdateMaxSkuPerBox(string customerId, int? splhValue)
        {
            _repos.UpdateCustomerSplhValue(customerId, "$MAXSKUPB", splhValue.HasValue ? splhValue.ToString() : null);
        }

        internal void UpdateSkuMinMaxPieces(string customerId, int? minPieces, int? maxPieces, int? orgMinPieces, int? orgMaxPieces)
        {
            if (minPieces > orgMaxPieces)
            {
                _repos.UpdateCustomerSplhValue(customerId, "_$MAXSKUPIECES", maxPieces.HasValue ? maxPieces.ToString() : null);
                _repos.UpdateCustomerSplhValue(customerId, "_$MINSKUPIECES", minPieces.HasValue ? minPieces.ToString() : null);
            }
            else
            {
                _repos.UpdateCustomerSplhValue(customerId, "_$MINSKUPIECES", minPieces.HasValue ? minPieces.ToString() : null);
                _repos.UpdateCustomerSplhValue(customerId, "_$MAXSKUPIECES", maxPieces.HasValue ? maxPieces.ToString() : null);
            }

        }


        /// <summary>
        /// This function returns list of customer preferred SKU cases.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CustomerSkuCase> GetCustomerSkuCaseList()
        {
            return _repos.GetCustomerSkuCaseList();
        }

        /// <summary>
        /// This function returns list of SKu cases.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SkuCase> GetSkuCaseList()
        {
            return _repos.GetSkuCaseList(null);
        }

        public SkuCase GetSkuCase(string skuCaseId)
        {
            return _repos.GetSkuCaseList(skuCaseId).SingleOrDefault();
        }

        /// <summary>
        /// This function updates an SKU case property.
        /// </summary>
        /// <param name="skuCase"> </param>
        public void AddorUpdateSkuCase(SkuCase skuCase)
        {
            _repos.AddorUpdateSkuCase(skuCase);
        }

        /// <summary>
        /// This function returns a list of packing rules for SKU case.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PackingRules> GetPackingRules()
        {
            return _repos.GetPackingRules();
        }

        public void DelCustSkuCasePrefereence(string customerId, string caseId)
        {
            _repos.DelCustSkuCasePrefereence(customerId, caseId);
        }

        public void AddCustSKuCasePreference(string customerId, string skuCaseId, string comments)
        {
            _repos.AddCustSKuCasePreference(customerId, skuCaseId, comments);
        }

        /// <summary>
        /// This function delete's SKU case ignorance against style.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="caseId"></param>
        public void DelCaseIgnorance(string style, string caseId)
        {
            _repos.DelCaseIgnorance(style, caseId);
        }

        public void InsertPackingRule(PackingRules model)
        {
            _repos.InsertPackingRule(model);
        }
    }
}