using System;
using System.Collections.ObjectModel;
using System.Data;
using Oracle.DataAccess.Client;

namespace EclipseLibrary.Oracle.Helpers
{
    /// <summary>
    /// A placeholder class to encapsulate the properties of an OracleParameter
    /// </summary>
    /// <remarks>
    /// This class is very similar to OracleParameter. It exists because the same OracleParameter cannot be added to multiple OracleCommands.
    /// </remarks>
    internal class BindParameter
    {
        public string ParameterName { get; set; }

        internal OracleParameter CreateOracleParameter()
        {
            var param = new OracleParameter();
            param.ParameterName = this.ParameterName;
            param.Size = this.Size;
            param.OracleDbType = this.OracleDbType;
            param.Value = this.Value;
            param.Direction = this.Direction;
            param.CollectionType = this.CollectionType;
            param.ArrayBindSize = this.ArrayBindSize;
            return param;
        }

        public ParameterDirection Direction { get; set; }

        public OracleDbType OracleDbType { get; set; }

        public object Value { get; set; }

        public int Size { get; set; }

        public OracleCollectionType CollectionType { get; set; }

        private Action<object> _outputValueUpdater;

        /// <summary>
        /// Non null for out parameters only. Sets the out value in the calling method variable
        /// </summary>
        public Action<object> OutputValueUpdater
        {
            get
            {
                return _outputValueUpdater;
            }
        }

        public void SetOutputValueUpdater<TValue>(Action<TValue> converter)
        {
            _outputValueUpdater = (object val) => converter((TValue) val);
        }

        /// <summary>
        /// Used to specify the max size of each returned string in string OutParameterAssociativeArray
        /// </summary>
        public int[] ArrayBindSize { get; set; }
    }

    /// <summary>
    /// Stores a list of BindParameters using case insensiive parameter names as the key
    /// </summary>
    internal class BindParameterCollection : KeyedCollection<string, BindParameter>
    {
        public BindParameterCollection()
            : base(StringComparer.InvariantCultureIgnoreCase, 4)
        {

        }
        protected override string GetKeyForItem(BindParameter item)
        {
            return item.ParameterName;
        }
    }

}
