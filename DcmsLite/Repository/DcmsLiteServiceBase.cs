using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using System.Web;

namespace DcmsMobile.DcmsLite.Repository
{
    public abstract class DcmsLiteServiceBase : IDisposable
    {
        internal abstract void Initialize(TraceContext ctx, string connectString, string userName, string clientInfo);

        public abstract void Dispose();

        internal abstract string GetBuildingDescription(string buildingId);
    }

    /// <summary>
    /// Use the implicit void constructor to instantiate and then immediately call Initialize()
    /// </summary>
    /// <typeparam name="TRepos"></typeparam>
    public abstract class DcmsLiteServiceBase<TRepos> : DcmsLiteServiceBase where TRepos : DcmsLiteRepositoryBase, new()
    {
        protected TRepos _repos;

        internal override void Initialize(TraceContext ctx, string connectString, string userName, string clientInfo)
        {
            _repos = new TRepos();
            _repos.Initialize(ctx, connectString, userName, clientInfo);
        }

        public override void Dispose()
        {
            if (_repos != null)
            {
                _repos.Dispose();
            }
        }

        private readonly string APPKEY_BUILDING = typeof(DcmsLiteServiceBase).FullName + "_building";

        internal override string GetBuildingDescription(string buildingId)
        {
            var dict = MemoryCache.Default[APPKEY_BUILDING] as ConcurrentDictionary<string, string>;
            if (dict == null)
            {
                dict = new ConcurrentDictionary<string, string>();
                MemoryCache.Default.Add(APPKEY_BUILDING, dict, new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(30)
                });
            }
            var result = dict.GetOrAdd(buildingId, key => _repos.GetBuildingDescription(key));
            return result;
        }

    }
}