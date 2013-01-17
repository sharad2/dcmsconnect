using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace EclipseLibrary.Oracle.Helpers
{
    /// <summary>
    /// Encapsulates parameters needed for DML array binding
    /// </summary>
    /// <remarks>
    /// <para>
    /// You implicitly construct an instance of this class when you use the <see cref="SqlBinder.Create(int)"/> overload and pass it the number of times your DML needs to execute.
    /// Then you create input parameters using one of the <c>Parameter</c> overloads. The DML query can have a <c>RETURNING</c> clause and you can receive the
    /// returned values by binding out parameters using one of the <c>OutParameter</c> overloads.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    ///internal void InsertBoxesForPickslip(IEnumerable<Box> boxes, IEnumerable<OrderedSku> orderedSku)
    ///{
    ///    // This DML query will be executed multiple times
    ///    const string QUERY_BOX = @"
    ///INSERT INTO <proxy />box
    ///(pickslip_id,
    ///ucc128_id,
    ///case_id,
    ///sequence_in_ps,
    ///min_pieces_per_box,
    ///vwh_id)
    ///VALUES
    ///(:PICKSLIP_ID,
    ///<proxy />GET_UCC128_ID,
    ///:CASE_ID,
    ///:sequence_in_ps,
    ///NULL,
    ///:vwh_id)
    ///RETURNING pickslip_id, ucc128_id INTO :pickslip_id_list, :ucc128_id_list
    ///";
    ///    // Parameters CASE_ID, PICKSLIP_ID, vwh_id will be bound using a single call to Parameters
    ///    var queryBox =
    ///        (from box in boxes
    ///            select new
    ///            {
    ///                CASE_ID = box.Case.CaseId,
    ///                PICKSLIP_ID = box.PickslipId,
    ///                vwh_id = orderedSku.First(p => box.AllSku.First().SkuId == p.SkuId).VwhId,  // VwhId of the first SKU in the box
    ///            }).ToArray();
    ///
    ///    // Create a binder which will execute the query queryBox.Length times
    ///    var binderBox = SqlBinder.Create(queryBox.Length)
    ///     .Parameters(queryBox)
    ///     .Parameter("sequence_in_ps", Enumerable.Range(orderedSku.Select(p => p.MaxSequenceInPs ?? 0).Max(), queryBox.Count).ToArray());
    ///
    ///    // Prepare to receive RETURNING values
    ///    IList<string> uccList = null;
    ///    IList<int> pickslipList = null;
    ///    binderBox.OutParameter("ucc128_id_list", (values) => uccList = values.ToList());
    ///    binderBox.OutParameter("pickslip_id_list", (values) => pickslipList = values.ToList());
    ///    _db.ExecuteDml(QUERY_BOX, binderBox);
    ///    
    ///   // The values returned are now available in uccList and pickslipList
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public class SqlBinderDmlArray : SqlBinderBase
    {
        private readonly int _arrayBindCount;

        /// <summary>
        /// Internal constructor. Use <see cref="SqlBinder.Create(int)"/> to create an instance.
        /// </summary>
        /// <param name="arrayBindCount"></param>
        /// <param name="actionName"></param>
        internal SqlBinderDmlArray(int arrayBindCount, string actionName)
            : base(actionName)
        {
            if (arrayBindCount <= 0)
            {
                throw new ArgumentOutOfRangeException("arrayBindCount");
            }
            _arrayBindCount = arrayBindCount;
        }

        /// <summary>
        /// This is set by the constructor. Cannot be changed thereafter.
        /// </summary>
        public int ArrayBindCount
        {
            get
            {
                return _arrayBindCount;
            }
        }

        /// <summary>
        /// Bind a string parameter
        /// </summary>
        /// <param name="field">Parameter name</param>
        /// <param name="values">Array of values. The number of values must be at least <see cref="ArrayBindCount"/></param>
        /// <returns></returns>
        public SqlBinderDmlArray Parameter(string field, IEnumerable<string> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            var param = GetBindParameter(field, ParameterDirection.Input);
            param.Value = values.Select(p => string.IsNullOrEmpty(p) ? DBNull.Value : (object)p).Take(this.ArrayBindCount).ToArray();
            param.OracleDbType = OracleDbType.Varchar2;
            return this;
        }

        /// <summary>
        /// Returns string values returned by RETURNING clause
        /// </summary>
        /// <param name="field">Output parameter name</param>
        /// <param name="setter">values => mylist = values.ToList(). Will not be called if no rows are </param>
        /// <returns>Self to enable chaining</returns>
        public SqlBinderDmlArray OutParameter(string field, Action<IEnumerable<string>> setter)
        {
            var param = GetBindParameter(field, ParameterDirection.Output);
            param.OracleDbType = OracleDbType.Varchar2;
            param.ArrayBindSize = Enumerable.Repeat(255, this.ArrayBindCount).ToArray();  // Max Size of each returned element
            param.SetOutputValueUpdater((OracleString[] p) => setter(p.Select(q => q.IsNull ? string.Empty : q.Value)));
            return this;
        }

        /// <summary>
        /// Returns integer values returned by RETURNING clause
        /// </summary>
        /// <param name="field">Output parameter name</param>
        /// <param name="setter">values => mylist = values.ToList()</param>
        /// <returns>Self to enable chaining</returns>
        public SqlBinderDmlArray OutParameter(string field, Action<IEnumerable<int>> setter)
        {
            var param = GetBindParameter(field, ParameterDirection.Output);
            param.OracleDbType = OracleDbType.Int32;
            param.SetOutputValueUpdater((OracleDecimal[] p) => setter(p.Select(q => q.IsNull ? 0 : q.ToInt32())));
            return this;
        }

        /// <summary>
        /// Returns Date values returned by RETURNING clause
        /// </summary>
        /// <param name="field">Output parameter name</param>
        /// <param name="setter">values => mylist = values.ToList()</param>
        /// <returns>Self to enable chaining</returns>
        public SqlBinderDmlArray OutParameter(string field, Action<IEnumerable<DateTime?>> setter)
        {
            var param = GetBindParameter(field, ParameterDirection.Output);
            param.OracleDbType = OracleDbType.Date;
            param.SetOutputValueUpdater((OracleDate[] p) => setter(p.Select(q => q.IsNull ? (DateTime?) null : q.Value)));
            return this;
        }

        /// <summary>
        /// Nullable Integer parameter
        /// </summary>
        /// <param name="field">Paramter name</param>
        /// <param name="values">Array of nullable integers</param>
        /// <returns></returns>
        public SqlBinderDmlArray Parameter(string field, IEnumerable<int?> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            var param = GetBindParameter(field, ParameterDirection.Input);
            param.Value = values.Select(p => p == null ? DBNull.Value : (object)p.Value).Take(this.ArrayBindCount).ToArray();
            param.OracleDbType = OracleDbType.Int32;
            return this;
        }

        /// <summary>
        /// Integer parameter
        /// </summary>
        /// <param name="field">Paramter name</param>
        /// <param name="values">Array of integers</param>
        /// <returns></returns>
        public SqlBinderDmlArray Parameter(string field, IEnumerable<int> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            var param = GetBindParameter(field, ParameterDirection.Input);
            param.Value = values.Take(this.ArrayBindCount).ToArray();
            param.OracleDbType = OracleDbType.Int32;
            return this;
        }

        /// <summary>
        /// Bind a Date parameter
        /// </summary>
        /// <param name="field">Parameter name</param>
        /// <param name="values">Array of dates</param>
        /// <returns></returns>
        public SqlBinderDmlArray Parameter(string field, IEnumerable<DateTime?> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            var param = GetBindParameter(field, ParameterDirection.Input);
            param.Value = values.Select(p => p == null ? DBNull.Value : (object)p.Value).Take(this.ArrayBindCount).ToArray();
            param.OracleDbType = OracleDbType.Date;
            return this;
        }

        /// <summary>
        /// Bind multiple parameters in one call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        ///var queryBox =
        ///    (from box in boxes
        ///        select new
        ///        {
        ///            CASE_ID = box.Case.CaseId,
        ///            PICKSLIP_ID = box.PickslipId,
        ///            vwh_id = box.VwhId
        ///        }).ToList();
        ///
        ///var binderBox = SqlBinder.Create(queryBox.Count);
        ///binderBox.Parameters(queryBox);
        ///_db.ExecuteDml(QUERY_BOX, binderBox);
        /// ]]>
        /// </code>
        /// </example>
        public SqlBinderDmlArray Parameters<T>(IEnumerable<T> values) where T : class
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                var paramValues = values.Select(p => prop.GetValue(p, null));
                if (prop.PropertyType == typeof(string))
                {
                    Parameter(prop.Name, paramValues.Cast<string>());
                }
                else if (prop.PropertyType == typeof(int))
                {
                    Parameter(prop.Name, paramValues.Cast<int>());
                }
                else if (prop.PropertyType == typeof(int?))
                {
                    Parameter(prop.Name, paramValues.Cast<int?>());
                }
                else if (prop.PropertyType == typeof(DateTime?))
                {
                    Parameter(prop.Name, paramValues.Cast<DateTime?>());
                }
                else
                {
                    throw new NotImplementedException(prop.PropertyType.FullName);
                }
            }

            return this;
        }

        /// <summary>
        /// If no rows are affected by the DML, we do not want to invoke the setters
        /// </summary>
        /// <param name="nRowsAffected"></param>
        /// <param name="parameters"></param>
        internal override void OnQueryExecuted(int nRowsAffected, OracleParameterCollection parameters)
        {
            if (nRowsAffected > 0)
            {
                base.OnQueryExecuted(nRowsAffected, parameters);
            }
        }
    }
}
