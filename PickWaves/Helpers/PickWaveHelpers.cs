using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DcmsMobile.PickWaves.Helpers
{
    internal static class PickWaveHelpers
    {
        /// <summary>
        /// Since reflection is expensive, we cache the reflection info in this collection.
        /// See http://stackoverflow.com/questions/330387/uses-of-keyedbytypecollection-in-net on advantages of KeyedByTypeCollection
        /// </summary>
        private static KeyedByTypeCollection<IDictionary> __coll = new KeyedByTypeCollection<IDictionary>();
        /// <summary>
        /// Returns a list of key value pairs. Key -> Enum Value; Value -> Attribute
        /// Example:
        /// <code>
        /// var attr = PickWaveHelpers.GetEnumMemberAttributes&lt;PickslipDimension, DisplayAttribute&gt;()[PickslipDimenstion.DcCancelDate];
        /// </code>
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <typeparam name="TAttr">Type of the attribute needed</typeparam>
        /// <returns></returns>
        public static IDictionary<TEnum, TAttr> GetEnumMemberAttributes<TEnum, TAttr>()
        {
            if (!__coll.Contains(typeof(Dictionary<TEnum, TAttr>)))
            {
                var query = from MemberInfo member in typeof(TEnum).GetMembers()
                            from TAttr attr in member.GetCustomAttributes(typeof(TAttr), false)
                            let dim = (TEnum)Enum.Parse(typeof(TEnum), member.Name)
                            select new KeyValuePair<TEnum, TAttr>(dim, attr);
                __coll.Add(query.ToDictionary(p => p.Key, p => p.Value));
            }
            return (IDictionary<TEnum, TAttr>)__coll[typeof(Dictionary<TEnum, TAttr>)];
        }

    }


}