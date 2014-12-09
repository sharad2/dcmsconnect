using DcmsMobile.REQ2.Areas.REQ2.SharedViews;
using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Routing;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
    /// <summary>
    /// These serve as flags to indicate what needs to be updated
    /// </summary>
    [Flags]
    public enum RequestProperties
    {
        None = 0x0,
        SourceVwhId = 0x1,
        QualityCode = 0x2,
        BuildingId = 0x4,
        PriceSeasonCode = 0x8,
        SewingPlantCode = 0x10,
        SourceAreaId = 0x20,
        Priority = 0x40,
        AllowOverPulling = 0x80,
        PackagingPreference = 0x100,
        DestinationArea = 0x200,
        TargetVwhId = 0x400,
        SaleTypeId = 0x800,
        CartonReceivedDate = 0x1000,
        Remarks = 0x2000,
        TargetQualityCode = 0x4000
    }

  

    public class ReqService : IDisposable
    {

        #region Intialization
        private readonly ReqRepository _repos;

        public ReqService(RequestContext ctx)
        {
            string module = "REQ2";
            var clientInfo = string.IsNullOrEmpty(ctx.HttpContext.Request.UserHostName) ?
                             ctx.HttpContext.Request.UserHostAddress : ctx.HttpContext.Request.UserHostName;

            _repos = new ReqRepository(ctx.HttpContext.User.Identity.Name, module, clientInfo, ctx.HttpContext.Trace);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }

        #endregion

        #region Get methods
        public IEnumerable<CartonArea> GetCartonAreas()
        {
            return _repos.GetCartonAreas(null);
        }

        public CartonArea GetCartonAreaInfo(string areaId)
        {
            return _repos.GetCartonAreas(areaId).FirstOrDefault();
        }

        public IEnumerable<CodeDescriptionModel> GetVwhList()
        {
            return _repos.GetVwhList();
        }

        public IEnumerable<CodeDescriptionModel> GetBuildingList()
        {
            return _repos.GetBuildingList();
        }

        public IEnumerable<CodeDescriptionModel> GetQualityCodes()
        {
            return _repos.GetQualityCodeList();
        }

        public IEnumerable<CodeDescriptionModel> GetSewingPlantCodes()
        {
            return _repos.GetSewingPlants();
        }

        public IEnumerable<CodeDescriptionModel> GetPriceSeasonCodes()
        {
            return _repos.GetPriceSeasonCodes();
        }


        public IEnumerable<RequestSkuModel> GetRequestSKUs(string ctnresvId)
        {
            return _repos.GetRequestSkus(ctnresvId);
        }

        public IEnumerable<CodeDescriptionModel> GetSaleTypeList()
        {
            return _repos.GetSaleTypeList();
        }


        public RequestModel GetRequestInfo(string ctnresvId)
        {
            if (string.IsNullOrEmpty(ctnresvId))
            {
                throw new ArgumentNullException("ctnresvId");
            }
            return _repos.GetRequests(ctnresvId, 1).SingleOrDefault();
        }

        public IEnumerable<RequestModel> GetRequests()
        {
            return _repos.GetRequests(null, 20);
        }


        public IEnumerable<AssignedCarton> GetAssignedCartons(string ctnresvId)
        {
            return _repos.GetAssignedCartons(ctnresvId);
        }
        public IEnumerable<CartonList> GetCartonList(string ctnresvId)
        {
            return _repos.GetCartonList(ctnresvId, 500);
        }

        //public string GetCtnRevId(string reqId)
        //{
        //    return _repos.GetCtnRevId(reqId);
        //}

        #endregion

        public void CreateCartonRequest(RequestModel model)
        {

            string sourceBuildingId = _repos.GetBuildingofArea(model.SourceAreaId);
            string destBuildingId = _repos.GetBuildingofArea(model.DestinationArea);
            if (!string.IsNullOrEmpty(sourceBuildingId) && !string.IsNullOrEmpty(destBuildingId))
            {
                if (sourceBuildingId != destBuildingId)
                {
                    throw new ProviderException("Selected FromArea and ToArea should be in a same building.");
                }
            }

            if (!string.IsNullOrEmpty(model.BuildingId))
            {
                if (!string.IsNullOrEmpty(sourceBuildingId))
                {
                    if (model.BuildingId != sourceBuildingId)
                    {
                        throw new ProviderException(string.Format("Selected source area {0} does not  belong to building {1}.",model.SourceAreaShortName,model.BuildingId));
                    }
                }
                sourceBuildingId = model.BuildingId;
            }
            model.BuildingId = sourceBuildingId;
            _repos.CreateCartonRequest(model);
        }

        public void UpdateCartonRequest(RequestModel model, RequestProperties propertiesToUpdate)
        {
            var modelToUpdate = _repos.GetRequests(model.CtnResvId, 1).SingleOrDefault();
            if (modelToUpdate == null)
            {
                throw new ProviderException("Invalid Request Id passed");
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.AllowOverPulling))
            {
                modelToUpdate.AllowOverPulling = model.AllowOverPulling;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.BuildingId))
            {
                modelToUpdate.BuildingId = model.BuildingId;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.CartonReceivedDate))
            {
                modelToUpdate.CartonReceivedDate = model.CartonReceivedDate;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.DestinationArea))
            {
                modelToUpdate.DestinationArea = model.DestinationArea;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.PackagingPreference))
            {
                modelToUpdate.PackagingPreferance = model.PackagingPreferance;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.PriceSeasonCode))
            {
                modelToUpdate.PriceSeasonCode = model.PriceSeasonCode;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.Priority))
            {
                modelToUpdate.Priority = model.Priority;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.QualityCode))
            {
                modelToUpdate.SourceQuality = model.SourceQuality;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.Remarks))
            {
                modelToUpdate.Remarks = model.Remarks;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.SaleTypeId))
            {
                modelToUpdate.SaleTypeId = model.SaleTypeId;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.SewingPlantCode))
            {
                modelToUpdate.SewingPlantCode = model.SewingPlantCode;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.SourceAreaId))
            {
                modelToUpdate.SourceAreaId = model.SourceAreaId;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.SourceVwhId))
            {
                modelToUpdate.SourceVwhId = model.SourceVwhId;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.TargetVwhId))
            {
                modelToUpdate.TargetVwhId = model.TargetVwhId;
            }

            if (propertiesToUpdate.HasFlag(RequestProperties.TargetQualityCode))
            {
                modelToUpdate.TargetQuality = model.TargetQuality;
            }
            CreateCartonRequest(modelToUpdate);
        }

        public void DeleteCartonRequest(string ctnresvId)
        {
            _repos.DeleteCartonRequest(ctnresvId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctnresvId"></param>
        /// <param name="sourceSkuId"></param>
        /// <param name="pieces"></param>
        /// <param name="targetSkuId"></param>
        public void AddSkutoRequest(string ctnresvId, int sourceSkuId, int pieces, int? targetSkuId)
        {
            var skusInRequest = _repos.GetRequestSkus(ctnresvId);

            // If source SKU has already been added to the request, its conversion SKU must match.
            var previouslyAddedSku = skusInRequest.Where(p => p.SourceSku.SkuId == sourceSkuId).FirstOrDefault();
            if (previouslyAddedSku != null)
            {
                bool problem;
                if (previouslyAddedSku.TargetSku == null)
                {
                    problem = targetSkuId != null;
                }
                else
                {
                    problem = previouslyAddedSku.TargetSku.SkuId != targetSkuId;
                }
                if (problem)
                {
                    // Problem
                    throw new ApplicationException(string.Format(@"
                                        SKU {0} has already been added to the request.
                                        Earlier it was being converted to {1}.<br/>
                                        You are now trying to convert it to a different SKU which is not allowed.",
                       previouslyAddedSku.SourceSku, previouslyAddedSku.TargetSku));
                }
            }

            _repos.AddSkutoRequest(ctnresvId, sourceSkuId, pieces, targetSkuId);
        }

        public void DeleteSkuFromRequest(int skuId, string ctnresvId)
        {
            _repos.DeleteSkuFromRequest(skuId, ctnresvId);
        }

        public void AssignCartons(string ctnresvId)
        {
            _repos.AssignCartons(ctnresvId);
        }

        public void UnAssignCartons(string ctnresvId)
        {
            _repos.UnAssignCartons(ctnresvId);
        }

        /// <summary>
        /// The results of this function must be cached
        /// </summary>
        /// <param name="style"></param>
        /// <param name="color"></param>
        /// <param name="dimension"></param>
        /// <param name="skuSize"></param>
        /// <returns></returns>
        public SkuModel GetSku(string style, string color, string dimension, string skuSize)
        {
            return _repos.GetSku(style, color, dimension, skuSize);
        }
    }
}
//$Id$