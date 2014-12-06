using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Routing;
using DcmsMobile.Repack.Models;

namespace DcmsMobile.Repack.Repository
{
    public enum QualityType
    {
        All,

        /// <summary>
        /// Order qualities
        /// </summary>
        Order,

        /// <summary>
        /// Received carton qualities
        /// </summary>
        Received,

        /// <summary>
        /// Get Quality Code order by descending.
        /// </summary>
        QualityOrderByDesc
    }
    public class RepackService : IDisposable
    {
        private readonly RepackRepository _repos;
        public RepackService(RequestContext requestContext)
        {
            _repos = new RepackRepository(requestContext);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }

        internal IDictionary<string, IList<VirtualWarehouse>> GetVirtualWarehouses()
        {
            return new Dictionary<string, IList<VirtualWarehouse>>(1)
            {
                {"", _repos.GetVirtualWarehouses()}
            };
        }

        internal IDictionary<string, IList<Quality>> GetQualities(QualityType qualityType)
        {
            IList<Quality> list;
            switch (qualityType)
            {
                case QualityType.All:
                    list = _repos.GetQualityCodes(null, null);
                    break;

                case QualityType.QualityOrderByDesc:
                    list = _repos.GetQualityCodes("D", null);
                    break;

                case QualityType.Order:
                    list = _repos.GetQualityCodes("O", null);
                    break;

                case QualityType.Received:
                    list = _repos.GetQualityCodes("R", null);
                    break;

                default:
                    throw new NotImplementedException();
            }
            return new Dictionary<string, IList<Quality>>(1)
            {
                {"", list}
            };
        }

        internal Quality GetQuality(QualityType qualityType)
        {

            IList<Quality> list;
            switch (qualityType)
            {
                case QualityType.All:
                    list = _repos.GetQualityCodes(null, 1);
                    break;

                case QualityType.Order:
                    list = _repos.GetQualityCodes("O", 1);
                    break;

                case QualityType.Received:
                    list = _repos.GetQualityCodes("R", 1);
                    break;

                default:
                    throw new NotImplementedException();
            }
            return list.FirstOrDefault();
        }

        internal IDictionary<string, IList<PriceSeason>> GetPriceSeasonCodes()
        {
            return new Dictionary<string, IList<PriceSeason>> { { "", _repos.GetPriceSeasonCodes() } };
        }

        internal IDictionary<string, IList<Printer>> GetZebraPrinters()
        {
            return new Dictionary<string, IList<Printer>> { { "", _repos.GetPrinters("ZEBRA") } };
        }
      
        internal IDictionary<string, IEnumerable<InventoryArea>> GetGroupedAreas(InventoryAreaFilters filters)
        {
            string groupText;
            string emptyGroupText;
            if (filters.HasFlag(InventoryAreaFilters.GroupByPalletRequirement))
            {
                groupText = "Pallet Areas";
                emptyGroupText = "Non Pallet Areas";
            }
            else if (filters.HasFlag(InventoryAreaFilters.GroupByUsability))
            {
                groupText = "Unusable Areas";
                emptyGroupText = "Usable Areas";
            }
            else
            {
                groupText = "";
                emptyGroupText = "";
            }

            var areas = _repos.GetInventoryAreas(filters);
            var dict = (from area in areas
                        group area by area.GroupingColumn into g
                        select new
                        {
                            Group = string.IsNullOrEmpty(g.Key) ? emptyGroupText : groupText,
                            Items = g.AsEnumerable()
                        }).ToDictionary(p => p.Group, q => q.Items);
            return dict;
        }

        internal IEnumerable<SewingPlant> GetGroupedSewingPlants()
        {
            var plants = _repos.GetSewingPlants();
            if (plants != null)
            {
                // Sort by numeric sewing plant code. Be prepared for error in case the code is not numeric
                plants = plants.OrderBy(p => string.Format("{0,10}", p.SewingPlantCode));
            }
            return plants;
        }

        /// <summary>
        /// If Source area is cancelled area, then pass source area is 'SHL'
        /// and remove carton from SRC_OPEN_CARTON.
        /// </summary>
        /// <param name="info"></param>
        /// <returns>
        /// Carton Id
        /// </returns>
        internal string[] RepackCarton(CartonRepackInfo info)
        {
                var result = _repos.RepackCarton(info);
                return result;
        }

        internal Sku GetSkuFromBarCode(string barCode)
        {
            // TODO: Cache this
            return _repos.GetSkuFromBarCode(barCode);
        }
    }
}

//$Id$
