using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    /// <summary>
    /// This is a value type which can hold a date or a string value.
    /// These values can be compared to each other and can be posted from MVC view.
    /// You construct it by passing a string or a date. Any other value throws an exception.
    /// Dates are displayed using the {0:d} format. Strings are displayed as is.
    /// </summary>
    [ModelBinder(typeof(DimensionValueBinder))]
    public struct DimensionValue : IEquatable<DimensionValue>
    {
        public static DimensionValue Empty
        {
            get
            {
                DimensionValue val;
                val._rawValue = null;
                val._type = DBNull.Value.GetType();
                return val;
            }
        }

        public static DimensionValue FromValue(object value)
        {
            if (value == null)
            {
                return Empty;
            }
            DimensionValue dv;
            dv._type = value.GetType();
            dv._type = Nullable.GetUnderlyingType(dv._type) ?? dv._type;
            dv._rawValue = value;
            return dv;
        }

        public static DimensionValue FromPostedValue(object value)
        {
            if (value == null) 
            {
                return Empty;
            }
            throw new NotImplementedException(value.ToString());

        }

        private  object _rawValue;
        private  Type _type;

        /// <summary>
        /// Post value shows date as YYYY-MM-DD
        /// </summary>
        public string PostValue1
        {
            get
            {
                if (_rawValue == null)
                {
                    return string.Empty;
                }
                if (_rawValue is DateTime)
                {
                    return string.Format("Sharad {0:yyyy-MM-dd}", _rawValue);
                }
                return _rawValue.ToString();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is DimensionValue)
            {
                return this.Equals((DimensionValue)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (_rawValue == null)
            {
                return 0;
            }
            return _rawValue.GetHashCode();
        }


        public bool Equals(DimensionValue other)
        {
            if (_rawValue == null)
            {
                return other._rawValue == null;
            }
            return _rawValue.Equals(other._rawValue);
        }

        public override string ToString()
        {
            if (_rawValue == null)
            {
                return string.Empty;
            }
            if (_type == typeof(DateTime))
            {
                return string.Format("{0:d}", _rawValue);
            }
            return _rawValue.ToString();
        }

    }

    internal class DimensionValueBinder : IModelBinder
    {

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            return DimensionValue.FromPostedValue(bindingContext.Model);
        }
    }
}