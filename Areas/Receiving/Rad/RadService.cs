using System;
using System.Collections.Generic;
using System.Web.Routing;


namespace DcmsMobile.Receiving.Areas.Receiving.Rad
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



        public bool AddUpdateSpotCheckSetting(string style, string color, string sewingPlantId, int? spotCheckPercent, bool enabled)
        {
            return _repos.AddUpdateSpotCheckSetting(style, color, sewingPlantId, spotCheckPercent, enabled);
        }


        public int DeleteSpotCheckSetting(string style,string color, string sewingPlantId)
        {
           return _repos.DeleteSpotCheckSetting(style,color,sewingPlantId);
        }

       
        #endregion


        public IEnumerable<SpotCheckArea> GetSpotCheckAreas()
        {
            return _repos.GetSpotCheckAreas();
        }

        internal IList<Tuple<string, string>> GetStyles(string searchId, string searchDescription)
        {
            return _repos.GetStyles(searchId, searchDescription);
        }

        internal IList<Tuple<string, string>> GetColors(string searchId, string searchDescription)
        {
            return _repos.GetColors(searchId, searchDescription);
        }
    }
}



//$Id$