using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using EclipseLibrary.Oracle.Helpers;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace EclipseLibrary.Oracle
{
    /// <summary>
    /// This serves as the class to use for parameter binding for PL/SQL blocks executed by <see cref="OracleDatastore"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="SqlBinder"/> can be passed as a parameter to the functions <see cref="OracleDatastore.ExecuteNonQuery"/>.
    /// </para>
    /// <para>
    /// RefCursor support. See <see cref="OutRefCursorParameter"/>
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// Following is a simple use of how to pass parameters to <see cref="OracleDatastore.ExecuteNonQuery"/>.
    /// The overloaded Parameter functions are used to specify the value of the parameter.
    /// </para>
    /// <code lang="c#">
    /// <![CDATA[
    ///public static void RemoveFromPallet(OracleDatabase db, string uccId)
    ///{
    ///    string QUERY = @"update <proxy />box  b
    ///set b.ia_id = (
    ///select i.ia_id from <proxy />iaconfig i where i.iaconfig_id = '$BADVERIFY'
    ///)
    ///where b.ucc128_id= :UccId";
    ///    var binder = SqlBinder.Create();
    ///    binder.Parameter("UccId", uccId);
    ///    db.ExecuteNonQuery(QUERY, binder);
    ///}
    /// ]]>
    /// </code>
    /// </example>
    public partial class SqlBinder : SqlBinderBase
    {
        #region Constructors

        /// <summary>
        /// SqlBinder.Create() is recommended instead of using this constructor directly
        /// </summary>
        /// <param name="actionName"></param>
        /// <remarks>
        /// This should be made private after warnings have been removed from applications.
        /// </remarks>
        public SqlBinder(string actionName)
            : base(actionName)
        {
        }

        /// <summary>
        /// Create a binder for retrieving the results of an SQL query
        /// <example>
        /// SqlBinder.Create(row => new RestockCarton
        ///  {
        ///    CartonId = row.GetString("CARTON_ID"),
        ///    ...
        ///  });
        /// </example>
        /// </summary>
        /// <typeparam name="T">Type of each row</typeparam>
        /// <param name="factory">Lambda which takes a row and returns a strongly typed object
        /// <example>
        ///row => new RestockCarton {
        ///        CartonId = row.GetString("CARTON_ID"),
        ///        QualityCode = row.GetString("QUALITY_CODE"),
        ///        SkuInCarton = new Sku {
        ///                                SkuId = row.GetInteger("SKU_ID"),
        ///                                Style = row.GetString("STYLE")
        ///                            }
        ///    }
        /// </example>
        /// </param>
        /// <returns></returns>
        public static SqlBinder<T> Create<T>(Func<OracleDataRow2, T> factory)
        {
            var frame = new StackFrame(1, false);
            var binder = new SqlBinder<T>(frame.GetMethod().Name);
            binder.Factory = factory;
            return binder;
        }

        /// <summary>
        /// Create a binder for executing PL/SQL blocks
        /// </summary>
        /// <returns></returns>
        public static SqlBinder Create()
        {
            var frame = new StackFrame(1, false);
            var binder = new SqlBinder(frame.GetMethod().Name);
            return binder;
        }

        /// <summary>
        /// Create a binder for for executing DML multiple times using DML array binding
        /// </summary>
        /// <param name="arrayBindCount"></param>
        /// <returns></returns>
        public static SqlBinderDmlArray Create(int arrayBindCount)
        {
            var frame = new StackFrame(1, false);
            var binder = new SqlBinderDmlArray(arrayBindCount, frame.GetMethod().Name);
            return binder;
        }
        #endregion

        #region Ref Cursor

        /// <summary>
        /// The parameter returns a ref cursor. Specify a factory which will convert the returned data to strongly typed objects
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="factory"></param>
        public void OutRefCursorParameter(string parameterName, Action<IEnumerable<OracleDataRow2>> factory)
        {
            var param = GetBindParameter(parameterName, ParameterDirection.Output);
            param.OracleDbType = OracleDbType.RefCursor;
            param.SetOutputValueUpdater((OracleRefCursor p) =>
            {
                using (var reader = p.GetDataReader())
                {
                    factory(ReadReader(reader));
                }
            });
            return;
        }

        private IEnumerable<OracleDataRow2> ReadReader(OracleDataReader reader)
        {
            var row = new OracleDataRow2(reader);
            while (reader.Read())
            {
                row.RefreshValues();
                yield return row;
            }
        }

        #endregion

        #region Scalar In Parameters
        /// <summary>
        /// Bind an input string parameter with specified max size. Should this be obsolete?
        /// </summary>
        /// <param name="field">Parameter Name</param>
        /// <param name="value">Parameter value</param>
        /// <returns>The binder to enable fluent syntax</returns>
        /// <remarks>
        /// Oracle Doc says: When sending a null parameter value to the database, the user must specify DBNull, not null.
        /// The null value in the system is an empty object that has no value. DBNull is used to represent null values.
        /// The user can also specify a null value by setting Status to OracleParameterStatus.NullValue. In this case, the provider sends a null value to the database.
        /// </remarks>
        public SqlBinder Parameter(string field, string value)
        {
            base.InParameterString(field, value);
            return this;
        }

        ///// <summary>
        ///// Bind string array parameter. Designed for the <![CDATA[<a>]]> element of XML Sql Query
        ///// </summary>
        ///// <param name="field"></param>
        ///// <param name="values"></param>
        ///// <returns></returns>
        ///// <example>
        ///// <code>
        ///// <![CDATA[
        /////public int DeleteBols(IEnumerable<string> ShippingIdList)
        /////{
        /////    const string QUERY = @"
        /////    BEGIN
        /////            UPDATE <proxy />PS PS
        /////            SET PS.SHIPPING_ID =''
        /////            WHERE PS.SHIPPING_ID IN(<a sep=','>:SHIPPINGIDLIST</a>)
        /////            and PS.transfer_date is null;
        /////            DELETE FROM <proxy />SHIP S
        /////            WHERE S.SHIPPING_ID IN (<a sep=','>:SHIPPINGIDLIST</a>)
        /////            ;
        /////    END;
        /////    ";
        /////    var binder = SqlBinder.Create().Parameter("SHIPPINGIDLIST", ShippingIdList);
        /////    return _db.ExecuteDml(QUERY, binder);
        /////}
        ///// ]]>
        ///// </code>
        ///// </example>
        //[Obsolete("Use ParameterXmlArray")]
        //public SqlBinder Parameter(string field, IEnumerable<string> values)
        //{
        //    base.ParameterXmlArray(field, values);
        //    return this;
        //}

        /// <summary>
        /// Bind a nullable integer parameter
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SqlBinder Parameter(string field, int? value)
        {
            base.InParameterInt(field, value);
            return this;
        }

        public SqlBinder Parameter(string field, int value)
        {
            base.InParameterInt(field, value);
            return this;
        }

        ///// <summary>
        ///// Bind an array of integer values
        ///// </summary>
        ///// <param name="field"></param>
        ///// <param name="values"></param>
        ///// <remarks>
        ///// <para>
        ///// array parameters are special handled to support XML array binding.
        ///// </para>
        ///// <example>
        ///// <![CDATA[
        /////public void FreezeBuckets(IEnumerable<int> bucketList)
        /////{
        /////    var QUERY = @"
        /////        UPDATE <proxy />BUCKET BKT SET BKT.FREEZE = 'Y' 
        /////            WHERE BKT.FREEZE IS NULL
        /////            <a pre="AND BKT.BUCKET_ID IN (" sep="," post=")">:BUCKETLIST</a>
        /////    ";
        /////    var binder = new SqlBinder("Freeze Buckets");
        /////    binder.Parameter("BUCKETLIST", bucketList);
        /////    _db.ExecuteNonQuery(QUERY, binder);
        /////}
        ///// ]]>
        ///// </example>
        ///// </remarks>
        //[Obsolete("Use ParameterXmlArray")]
        //public SqlBinder Parameter(string field, IEnumerable<int> values)
        //{
        //    base.ParameterXmlArray(field, values);
        //    return this;
        //}

        ///// <summary>
        ///// Datatype is set to Int16. Value is set to null for false, 1 for true
        ///// </summary>
        ///// <param name="field"></param>
        ///// <param name="value"></param>
        ///// <returns></returns>
        ///// <remarks>
        ///// This parameter is primarily intended for XPath expressions. It is not very useful for binding to an Oracle query.
        ///// </remarks>
        //[Obsolete("Use ParameterXPath")]
        //public SqlBinder Parameter(string field, bool value)
        //{
        //    base.ParameterXPath(field, value);
        //    return this;
        //}

        /// <summary>
        /// Bind a nullable date parameter
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SqlBinder Parameter(string field, DateTime? value)
        {
            base.InParameterDateTime(field, value);
            return this;
        }

        public SqlBinder Parameter(string field, DateTime value)
        {
            base.InParameterDateTime(field, value);
            return this;
        }

        /// <summary>
        /// Bind a floating point parameter
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SqlBinder Parameter(string field, decimal? value)
        {
            base.InParameterDecimal(field, value);
            return this;
        }

        public SqlBinder Parameter(string field, decimal value)
        {
            base.InParameterDecimal(field, value);
            return this;
        }

        /// <summary>
        /// Bind an interval value
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SqlBinder Parameter(string field, TimeSpan? value)
        {
            base.InParameterIntervalDS(field, value);
            return this;
        }

        /// <summary>
        /// Bind a TimeStampTZ value
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SqlBinder Parameter(string field, DateTimeOffset? value)
        {
            base.InParameterTimeStampTZ(field, value);
            return this;
        }

        #endregion

        #region Scalar Out Parameters

        public SqlBinder OutParameter(string parameterName, Action<string> setter)
        {
            var param = GetBindParameter(parameterName, ParameterDirection.Output);
            param.OracleDbType = OracleDbType.Varchar2;
            param.Size = 255;
            param.SetOutputValueUpdater((OracleString val) => setter(val.IsNull ? string.Empty : val.Value));
            return this;
        }

        public SqlBinder OutParameter(string parameterName, Action<int?> setter)
        {
            var param = GetBindParameter(parameterName, ParameterDirection.Output);
            param.OracleDbType = OracleDbType.Int32;
            param.SetOutputValueUpdater((OracleDecimal val) => setter(val.IsNull ? (int?)null : val.ToInt32()));
            return this;
        }

        //[Obsolete("Use the nullable int setter")]
        //public SqlBinder OutParameter(string parameterName, Action<int> setter)
        //{
        //    var param = GetBindParameter(parameterName, ParameterDirection.Output);
        //    param.OracleDbType = OracleDbType.Int32;
        //    param.SetOutputValueUpdater((OracleDecimal val) => setter(val.IsNull ? 0 : val.ToInt32()));
        //    return this;
        //}

        public SqlBinder OutParameter(string parameterName, Action<DateTime?> setter)
        {
            var param = GetBindParameter(parameterName, ParameterDirection.Output);
            param.OracleDbType = OracleDbType.Date;
            param.SetOutputValueUpdater((OracleDate val) => setter(val.IsNull ? (DateTime?)null : val.Value));
            return this;
        }

        public SqlBinder OutParameter(string parameterName, Action<TimeSpan?> setter)
        {
            var param = GetBindParameter(parameterName, ParameterDirection.Output);
            param.OracleDbType = OracleDbType.IntervalDS;
            param.SetOutputValueUpdater((OracleIntervalDS val) => setter(val.IsNull ? (TimeSpan?)null : val.Value));
            return this;
        }

        #endregion

        #region Associative Array Parameters

        /// <summary>
        /// Passes an aray to a PL/SQL Procedure
        /// </summary>
        /// <param name="field"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Inspired by http://www.oracle.com/technetwork/issue-archive/2007/07-jan/o17odp-093600.html
        /// See <see cref="OutParameterAssociativeArray"/> for an example
        /// </para>
        /// </remarks>
        public SqlBinder ParameterAssociativeArray(string field, ICollection<string> values)
        {
            var param = GetBindParameter(field, ParameterDirection.Input);
            if (values == null)
            {
                param.Size = 0;
                param.Value = DBNull.Value;
            }
            else
            {
                param.Size = values.Count;
                param.Value = values;
            }
            param.OracleDbType = OracleDbType.Varchar2;
            param.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            return this;
        }

        /// <summary>
        /// Passes an integer array to a PL/SQL procedure
        /// </summary>
        /// <param name="field"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <remarks>
        /// See <see cref="OutParameterAssociativeArray"/> for an example.
        /// </remarks>
        public SqlBinder ParameterAssociativeArray(string field, ICollection<int?> values)
        {
            var param = GetBindParameter(field, ParameterDirection.Input);
            param.OracleDbType = OracleDbType.Int32;
            param.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            param.Size = values.Count;
            param.Value = values ?? (object)DBNull.Value;
            return this;
        }

        /// <summary>
        /// Bind an output PL/SQL associative array parameter
        /// </summary>
        /// <param name="field"></param>
        /// <param name="setter"></param>
        /// <param name="maxElements">Maximum number of elements which can be returned. ORA-06513 raised if this is too small.</param>
        /// <param name="maxSizePerElement">Maximum size of each element. ORA-06502 raised if this is too small.</param>
        /// <returns></returns>
        /// <remarks>
        /// Null values are returned as empty strings within the array.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        ///create or replace package sharad_test is
        ///  -- Public type declarations
        ///  type string_list_t is table of varchar2(256) index by pls_integer;
        ///  -- Public function and procedure declarations
        ///  function test_func(a in string_list_t) return string_list_t;
        ///end sharad_test;
        /// ]]>
        /// </code>
        /// <code>
        /// <![CDATA[
        ///        public IEnumerable<Area> GetInventoryAreas()
        ///        {
        ///            const string QUERY = @"
        ///BEGIN
        ///:a := sharad_test.test_func(:a);
        ///END;
        ///            ";
        ///            var arr = new[] { "a", "b" };
        ///            var binder = SqlBinder.Create()
        ///                .ParameterAssociativeArray("a", arr)
        ///                .OutParameterAssociativeArray("a", val => arr = val.ToArray(), Enumerable.Repeat<int>(255, 3).ToArray());
        ///
        ///            _db.ExecuteNonQuery(QUERY, binder);
        ///        }
        /// ]]>
        /// </code>
        /// </example>
        public SqlBinder OutParameterAssociativeArray(string field, Action<ICollection<string>> setter, int maxElements, int maxSizePerElement)
        {
            var param = GetBindParameter(field, ParameterDirection.Output);
            param.OracleDbType = OracleDbType.Varchar2;
            param.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            param.ArrayBindSize = Enumerable.Repeat(maxSizePerElement, maxElements).ToArray();  // Max Size of each returned element
            param.Size = maxElements;  // Maximum number of elements that can be returned
            param.SetOutputValueUpdater((OracleString[] p) => setter(p.Select(q => q.IsNull ? string.Empty : q.Value).ToArray()));
            return this;
        }

        /// <summary>
        /// The output associcative array can have a maximum of 255 elements, each element having a max length of 255.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="setter"></param>
        /// <param name="maxElements"></param>
        /// <returns></returns>
        public SqlBinder OutParameterAssociativeArray(string field, Action<ICollection<string>> setter, int maxElements)
        {
            return OutParameterAssociativeArray(field, setter, maxElements, 255);
        }

        /// <summary>
        /// Receives an integer array from a PL/SQL procedure
        /// </summary>
        /// <param name="field"></param>
        /// <param name="setter"></param>
        /// <param name="maxElements"></param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        ///public IEnumerable<Area> GetInventoryAreas()
        ///{
        ///    const string QUERY = @"
        ///BEGIN
        ///:a := sharad_test.test_func(:a);
        ///END;
        ///    ";
        ///    var arr = new int?[] { 1, 2 };
        ///    var binder = SqlBinder.Create()
        ///        .ParameterAssociativeArray("a", arr)
        ///        .OutParameterAssociativeArray("a", val => arr = val.ToArray());
        ///    _db.ExecuteNonQuery(QUERY, binder);
        ///    throw new NotImplementedException();
        ///}
        /// ]]>
        /// </code>
        /// </example>
        public SqlBinder OutParameterAssociativeArray(string field, Action<ICollection<int?>> setter, int maxElements)
        {
            var param = GetBindParameter(field, ParameterDirection.Output);
            param.OracleDbType = OracleDbType.Int32;
            param.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            param.Size = maxElements;  // Maximum number of array elements that can be returned

            // Non integer values raise exception.
            param.SetOutputValueUpdater((OracleDecimal[] p) => setter(p.Select(q => q.IsNull ? (int?)null : q.ToInt32()).ToArray()));
            return this;
        }

        public SqlBinder OutParameterAssociativeArray(string field, Action<ICollection<int>> setter, int maxElements)
        {
            var param = GetBindParameter(field, ParameterDirection.Output);
            param.OracleDbType = OracleDbType.Int32;
            param.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            param.Size = maxElements;  // Maximum number of array elements that can be returned

            // Null values or non integer values raise exception.
            param.SetOutputValueUpdater((OracleDecimal[] p) => setter(p.Select(q => q.ToInt32()).ToArray()));
            return this;
        }

        #endregion

    }

}
