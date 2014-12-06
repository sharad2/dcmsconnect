using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Oracle.ManagedDataAccess.Client;

namespace EclipseLibrary.Oracle.Helpers
{
    /// <summary>
    /// Provides a way to convert results returned by a query into strongly typed objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// </remarks>
    public class SqlBinder<T> : SqlBinderBase
    {
        #region Constructors
        internal SqlBinder(string actionName)
            : base(actionName)
        {

        }

        #endregion

        #region Parameter chaining
        public SqlBinder<T> Parameter(string field, string value)
        {
            base.InParameterString(field, value);
            return this;
        }


        public SqlBinder<T> Parameter(string field, int? value)
        {
            base.InParameterInt(field, value);
            return this;
        }

        public SqlBinder<T> Parameter(string field, int value)
        {
            base.InParameterInt(field, value);
            return this;
        }

        public SqlBinder<T> Parameter(string field, DateTime? value)
        {
            base.InParameterDateTime(field, value);
            return this;
        }

        public SqlBinder<T> Parameter(string field, DateTime value)
        {
            base.InParameterDateTime(field, value);
            return this;
        }

        public SqlBinder<T> Parameter(string field, DateTimeOffset? value)
        {
            base.InParameterTimeStampTZ(field, value);
            return this;
        }

        public SqlBinder<T> Parameter(string field, decimal? value)
        {
            base.InParameterDecimal(field, value);
            return this;
        }

        public SqlBinder<T> Parameter(string field, decimal value)
        {
            base.InParameterDecimal(field, value);
            return this;
        }

        public SqlBinder<T> OutRefCursorParameter(string parameterName)
        {
            var param = GetBindParameter(parameterName, ParameterDirection.Output);
            param.OracleDbType = OracleDbType.RefCursor;
            return this;
        }

        #endregion

        #region Factory

        private Func<OracleDataRow2, T> _factory;

        /// <summary>
        /// The factory can be set once via one of the <see cref="SqlBinder.Create"/> overloads.
        /// </summary>
        public Func<OracleDataRow2, T> Factory
        {
            get
            {
                return _factory;
            }
            internal set
            {
                _factory = value;
            }
        }

        /// <summary>
        /// After the reader is executed, this function is responsible for caling the factory for each row retrieved
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <remarks>
        /// Only in DEBUG mode, it ensures that all columns retrieved by the query have been accessed.
        /// The finally block ensures that an exception has not been thrown and then proceeds to call <see cref="OracleDataRow2.DebugCheck"/>.
        /// http://stackoverflow.com/questions/2788793/how-to-get-the-current-exception-without-having-passing-the-variable
        /// </remarks>
        internal IEnumerable<T> MapRows(OracleDataReader reader)
        {
            Debug.Assert(_factory != null);
            return DoMapRows(reader, _factory);

            //            // Compatibility code
            //            var list = new List<T>();
            //            while (reader.Read())
            //            {
            //#pragma warning disable 612
            //                // Backward compatibility code. Obsolete warning disabled
            //                var dict = new OracleDataRow(_mapper, reader);
            //                list.Add(_mapper.Engine.Map<IOracleDataRow, T>(dict));
            //#pragma warning restore 612
            //            }
            //            return list;
        }

        /// <summary>
        /// After the reader is executed, this function is responsible for caling the factory for each row retrieved
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        /// <remarks>
        /// Only in DEBUG mode, it ensures that all columns retrieved by the query have been accessed.
        /// The finally block ensures that an exception has not been thrown and then proceeds to call <see cref="OracleDataRow2.DebugCheck"/>.
        /// http://stackoverflow.com/questions/2788793/how-to-get-the-current-exception-without-having-passing-the-variable
        /// </remarks>
        protected IEnumerable<T> DoMapRows(OracleDataReader reader, Func<OracleDataRow2, T> factory)
        {
#if DEBUG
            var inException = false;
            EventHandler<FirstChanceExceptionEventArgs> x = (object s, FirstChanceExceptionEventArgs e) => inException = true;
#endif
            var row = new OracleDataRow2(reader);
            try
            {
#if DEBUG
                AppDomain.CurrentDomain.FirstChanceException += x;
#endif
                while (reader.Read())
                {
                    row.RefreshValues();
                    yield return factory(row);
                }
            }
            finally
            {
#if DEBUG
                if (!inException)
                {
                    row.DebugCheck();
                }
                AppDomain.CurrentDomain.FirstChanceException -= x;
#endif
            }
        }
        #endregion

        //        #region Obsolete
        //        [Obsolete]
        //        [EditorBrowsable(EditorBrowsableState.Never)]
        //        private OracleMapperConfiguration _mapper;

        //        [Obsolete]
        //        [EditorBrowsable(EditorBrowsableState.Never)]
        //        internal SqlBinder(OracleMapperConfiguration mapper)
        //            : base("")
        //        {
        //            _mapper = mapper;
        //        }

        //        [Obsolete]
        //        [EditorBrowsable(EditorBrowsableState.Never)]
        //        public SqlBinder(string actionName, bool tolerateMissingParams = false)
        //            : base(actionName)
        //        {
        //            this.TolerateMissingParams = tolerateMissingParams;
        //        }

        //        [Obsolete("Is anyone using this?")]
        //        [EditorBrowsable(EditorBrowsableState.Never)]
        //        public SqlBinder<T> Parameter<TValue>(string field, TValue value)
        //        {
        //            throw new NotImplementedException();
        //            //base.Parameter(field, value);
        //            //return this;
        //        }

        //        [Obsolete]
        //        [EditorBrowsable(EditorBrowsableState.Never)]
        //        private string _query;

        //        /// <summary>
        //        /// Normally you set the query using <see cref="CreateMapper(string, Action{OracleMapperConfiguration})"/>. To execute another query using
        //        /// the same binder, you can set it here.
        //        /// </summary>
        //        [Obsolete]
        //        [EditorBrowsable(EditorBrowsableState.Never)]
        //        public string Query
        //        {
        //            get
        //            {
        //                return _query;
        //            }
        //            set
        //            {
        //                _query = value;
        //            }
        //        }

        //        [Obsolete]
        //        [EditorBrowsable(EditorBrowsableState.Never)]
        //        public void CreateMapper(string query)
        //        {
        //            //_query = query;
        //            CreateMapper(query, config => config.CreateMap<IOracleDataRow, T>().ConvertUsing(row => row.GetValue<T>(0)));
        //        }

        //        /// <summary>
        //        /// The dictionary key against which auto mapper maps are stored
        //        /// </summary>
        //        [Obsolete]
        //        [EditorBrowsable(EditorBrowsableState.Never)]
        //        private string _key;

        //        /// <summary>
        //        /// All the maps are added to a concurrent dictionary which is preserved in the application cache.
        //        /// All maps expire after a certain time and map creation starts all over again.
        //        /// </summary>
        //        /// <param name="query">The query associated with the binder</param>
        //        /// <param name="initializer"></param>
        //        /// <remarks>
        //        /// All maps are stored in application memory within a single dictionary. The key of each map within the dictionary is
        //        /// an MD5 hash created based on the query and the binder type.
        //        /// </remarks>
        //        [Obsolete]
        //        [EditorBrowsable(EditorBrowsableState.Never)]
        //        public void CreateMapper(string query, Action<OracleMapperConfiguration> initializer)
        //        {
        //#if DEBUG
        //            // In debug mode we expire very fast so that we can trap expiry bugs
        //            var expiry = TimeSpan.FromMinutes(1);
        //#else
        //            var expiry = TimeSpan.FromHours(1);
        //#endif
        //            if (string.IsNullOrEmpty(query))
        //            {
        //                throw new ArgumentNullException("query");
        //            }
        //            if (initializer == null)
        //            {
        //                throw new ArgumentNullException("initializer");
        //            }

        //            // Use SqlBinder class GuID as the cache key for the dictionary. We discard the dictionary frequently so that all maps can refresh periodically.
        //            // This strategy ensures that only active maps are retained.
        //            var keyDict = typeof(SqlBinder).GUID.ToString();
        //            var dict = MemoryCache.Default[keyDict] as ConcurrentDictionary<string, OracleMapperConfiguration>;
        //            if (dict == null)
        //            {
        //                dict = new ConcurrentDictionary<string, OracleMapperConfiguration>();
        //                MemoryCache.Default.Add(keyDict, dict, new CacheItemPolicy
        //                {
        //                    SlidingExpiration = expiry
        //                });
        //            }

        //            // Construct the MD5 key for the map. Query text and binder type is considered while creating the hash.
        //            // Inspired by http://www.techlicity.com/blog/dotnet-hash-algorithms.html
        //            var ue = new UnicodeEncoding();
        //            if (!string.IsNullOrEmpty(_key))
        //            {
        //                throw new InvalidOperationException("CreateMapper can only be called once per binder");
        //            }
        //            using (var md5 = new MD5CryptoServiceProvider())
        //            {
        //                _key = md5.ComputeHash(ue.GetBytes(query)).Concat(typeof(T).GUID.ToByteArray()).Aggregate(string.Empty, (result, next) => result + string.Format("{0:x2}", next));
        //            }
        //            OracleMapperConfiguration mapper;
        //            if (!dict.TryGetValue(_key, out mapper))
        //            {
        //                // Use this opportunity to clear out unused maps
        //                var oldMaps = dict.Where(p => DateTime.Now - p.Value.LastUsedTime > expiry).Select(p => p.Key).ToArray();
        //                foreach (var oldmap in oldMaps)
        //                {
        //                    dict.TryRemove(oldmap, out mapper);
        //                }
        //                mapper = new OracleMapperConfiguration(typeof(T).FullName + "|" + query);
        //                initializer(mapper);
        //                // Adding may fail because some other process may have just created the map. This is OK with us.
        //                dict.TryAdd(_key, mapper);
        //            }
        //            _query = query;
        //            _mapper = mapper;
        //        }


        //        #endregion

    }
}
