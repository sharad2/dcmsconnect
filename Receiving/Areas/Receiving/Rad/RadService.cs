using System;
using System.Collections.Generic;
using System.Web.Routing;
using DcmsMobile.Receiving.Models.Rad;

namespace DcmsMobile.Receiving.Repository
{


    public class RadService : IDisposable
    {
        #region Intialization

        private readonly RadRepository _repos;

        public RadService(RequestContext ctx)
        {
            _repos = new RadRepository(ctx);
        }
        public void Dispose()
        {
            var dis = _repos as IDisposable;
            if (dis != null)
            {
                dis.Dispose();
            }
        }

        #endregion

        public IEnumerable<SewingPlant> GetSewingPlants()
        {
            return _repos.GetSewingPlants();
        }

        #region SpotCheck

        public IList<SpotCheckConfiguration> GetSpotCheckList()
        {
            return _repos.GetSpotCheckList();
        }

        [Obsolete]
        public void SetSpotCheckPercentage(SpotCheckConfiguration spotCheck)
        {

            _repos.SetSpotCheckPercentage(spotCheck);
        }

        public void AddUpdateSpotCheckSetting(string style, string color, string sewingPlantId, int? spotCheckPercent, bool enabled)
        {

            _repos.AddUpdateSpotCheckSetting(style, color, sewingPlantId, spotCheckPercent, enabled);
        }


        public void DeleteSpotCheckSetting(string style, string sewingPlantId)
        {

            _repos.DeleteSpotCheckSetting(style,sewingPlantId);
        }

       
        #endregion

        //[Obsolete]
        //public int QueryCount
        //{
        //    get
        //    {
        //        return _repos.QueryCount;
        //    }
        //}

        public IEnumerable<SpotCheckArea> GetSpotCheckAreas()
        {
            return _repos.GetSpotCheckAreas();
        }
    }
}



//$Id$