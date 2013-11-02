using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web;
using DcmsMobile.REQ2.Models;

namespace DcmsMobile.REQ2.Repository
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
        IsConversionRequest = 0x80,
        DestinationArea = 0x200,
        TargetVwhId = 0x400,
        CartonReceivedDate = 0x1000,
        Remarks = 0x2000,
        TargetQualityCode = 0x4000
    }

    public class ReqService : IDisposable
    {
        #region Intialization

        private readonly ReqRepository _repos;

        /// <summary>
        /// For unit tests. 
        /// </summary>
        public ReqService(ReqRepository repos)
        {
            _repos = repos;
        }

        /// <summary>
        /// Used to store destination area of intransit cartons until they are received
        /// </summary>      
        public ReqService(TraceContext ctx, string connectString, string userName, string clientInfo, string moduleName)
        {
            _repos = new ReqRepository(ctx, connectString, userName, clientInfo, moduleName);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }

        #endregion


        #region Get methods


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


        public IEnumerable<RequestSku> GetRequestSKUs(string ctnresvId)
        {
            return _repos.GetRequestSkus(ctnresvId);
        }

        public Request GetRequestInfo(string ctnresvId)
        {
            if (string.IsNullOrEmpty(ctnresvId))
            {
                throw new ArgumentNullException("ctnresvId");
            }
            return _repos.GetRequests(ctnresvId, 1).SingleOrDefault();
        }

        public IEnumerable<Request> GetRequests()
        {
            return _repos.GetRequests(null, 20);
        }

        public IEnumerable<CartonList> GetCartonList(string ctnresvId)
        {
            return _repos.GetCartonList(ctnresvId);
        }
        
        #endregion

        /// <summary>
        /// This method create new request as passed properties of model. 
        /// </summary>
        /// <param name="model"> 
        /// </param>
        public string CreateCartonRequest(Request model)
        {
            return _repos.CreateRequest(model);
        }

        public void UpdateCartonRequest(Request updatedRequest, RequestProperties propertiesToUpdate)
        {
            var modelToUpdate = _repos.GetRequests(updatedRequest.CtnResvId, 1).SingleOrDefault();
            if (modelToUpdate == null)
            {
                throw new ProviderException("Invalid Request Id passed");
            }

            if (propertiesToUpdate.HasFlag(RequestProperties.BuildingId))
            {
                modelToUpdate.BuildingId = updatedRequest.BuildingId;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.CartonReceivedDate))
            {
                modelToUpdate.CartonReceivedDate = updatedRequest.CartonReceivedDate;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.DestinationArea))
            {
                modelToUpdate.DestinationArea = updatedRequest.DestinationArea;
            }

            if (propertiesToUpdate.HasFlag(RequestProperties.PriceSeasonCode))
            {
                modelToUpdate.PriceSeasonCode = updatedRequest.PriceSeasonCode;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.Priority))
            {
                modelToUpdate.Priority = updatedRequest.Priority;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.QualityCode))
            {
                modelToUpdate.SourceQuality = updatedRequest.SourceQuality;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.Remarks))
            {
                modelToUpdate.Remarks = updatedRequest.Remarks;
            }

            if (propertiesToUpdate.HasFlag(RequestProperties.SewingPlantCode))
            {
                modelToUpdate.SewingPlantCode = updatedRequest.SewingPlantCode;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.SourceAreaId))
            {
                modelToUpdate.SourceAreaId = updatedRequest.SourceAreaId;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.SourceVwhId))
            {
                modelToUpdate.SourceVwhId = updatedRequest.SourceVwhId;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.TargetVwhId))
            {
                modelToUpdate.TargetVwhId = updatedRequest.TargetVwhId;
            }

            if (propertiesToUpdate.HasFlag(RequestProperties.TargetQualityCode))
            {
                modelToUpdate.TargetQuality = updatedRequest.TargetQuality;
            }
            if (propertiesToUpdate.HasFlag(RequestProperties.IsConversionRequest))
            {
                modelToUpdate.IsConversionRequest = updatedRequest.IsConversionRequest;
            }
            _repos.UpdateRequest(modelToUpdate);
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
        public int AddSkutoRequest(string ctnresvId, int sourceSkuId, int pieces, int? targetSkuId)
        {
            return _repos.AddSkutoRequest(ctnresvId, sourceSkuId, pieces, targetSkuId);
        }

        public void DeleteSkuFromRequest(int skuId, string ctnresvId)
        {
            _repos.DeleteSkuFromRequest(skuId, ctnresvId);
        }

        public int AssignCartons(string ctnresvId)
        {
           return _repos.AssignCartons(ctnresvId);
        }

        public int UnAssignCartons(string ctnresvId)
        {
            return _repos.UnAssignCartons(ctnresvId);
        }

        /// <summary>
        /// The results of this function must be cached
        /// </summary>
        /// <param name="style"></param>
        /// <param name="color"></param>
        /// <param name="dimension"></param>
        /// <param name="skuSize"></param>
        /// <returns></returns>
        public Sku GetSku(string style, string color, string dimension, string skuSize)
        {
            return _repos.GetSku(style, color, dimension, skuSize);
        }

        public IEnumerable<CartonArea> GetCartonAreas(string buildingId)
        {
            return _repos.GetCartonAreas(buildingId);
        }
    }
}
//$Id$