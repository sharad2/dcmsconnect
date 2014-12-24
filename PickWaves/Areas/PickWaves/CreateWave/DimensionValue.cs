using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    /// <summary>
    /// This is a value type which can hold a date or a string value.
    /// These values can be compared to each other and can be posted from MVC view.
    /// You construct it by passing a string or a date object.
    /// Dates are displayed using the {0:d} format. Strings are displayed as is.
    /// You can post a DimensionValue and it will properly reconstruct itself to date or string.
    /// </summary>
    [ModelBinder(typeof(DimensionValueBinder))]
    public struct DimensionValue : IEquatable<DimensionValue>
    {
        private const string FORMAT_STRING = "d";
        
        /// <summary>
        /// Returns and empty value
        /// </summary>
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

        /// <summary>
        /// Called during mapping of repository value to model value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// If it looks like a date, we will assume it is a date.
        /// Called during model binding
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DimensionValue FromString(string value)
        {
            if (value == null) 
            {
                return Empty;
            }
            DimensionValue dv;
            DateTime date;
            if (DateTime.TryParseExact(value, FORMAT_STRING, CultureInfo.CurrentCulture, DateTimeStyles.None, out date))
            {
                dv._rawValue = date;
                dv._type = typeof(DateTime);
            }
            else
            {
                // Assume string
                dv._rawValue = value;
                dv._type = typeof(string);
            }
            return dv;
        }

        private  object _rawValue;
        private  Type _type;

        /// <summary>
        /// Returns true if the contained value is a date.
        /// </summary>
        public bool IsDate
        {
            get
            {
                return _type == typeof(DateTime);
            }
        }

        /// <summary>
        /// Always returns null if value is not date
        /// </summary>
        public DateTime? DateValue
        {
            get
            {
                if (!IsDate)
                {
                    return null;
                }
                return Convert.ToDateTime(_rawValue);
            }
        }

        #region IEquatable
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
            if (_type != other._type)
            {
                return false;
            }
            if (_rawValue == null)
            {
                return other._rawValue == null;
            }
            return _rawValue.Equals(other._rawValue);
        }
        #endregion

        /// <summary>
        /// Used to display the value in the view
        /// </summary>
        /// <returns></returns>
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

        public static bool operator == (DimensionValue a, DimensionValue b)
        {
            //throw new NotImplementedException();
            return a.Equals(b);
        }

        public static bool operator !=(DimensionValue a, DimensionValue b)
        {
            //throw new NotImplementedException();
            return !a.Equals(b);
        }

    }

    /// <summary>
    /// Invoked to assign a posted value to a DimensionValue
    /// </summary>
    internal class DimensionValueBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var x = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (x == null || String.IsNullOrWhiteSpace(x.AttemptedValue))
            {
                return DimensionValue.Empty;
            }
            return DimensionValue.FromString(x.AttemptedValue);
        }
    }
}