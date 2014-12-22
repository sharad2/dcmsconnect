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
            var row = new OracleDataRow2(reader.GetSchemaTable());
            try
            {
#if DEBUG
                AppDomain.CurrentDomain.FirstChanceException += x;
#endif
                while (reader.Read())
                {
                    row.SetValues(reader);
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

    }
}
