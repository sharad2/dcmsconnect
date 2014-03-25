using System.Collections.Generic;
using EclipseLibrary.Oracle.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DcmsMobile.BoxPick.Tests.Fakes
{
    public class MockOracleDataRow : IOracleDataRow
    {
        private readonly IDictionary<string, object> _dict;

        public MockOracleDataRow(IDictionary<string, object> dict)
        {
            _dict = dict;
        }

        public object this[string fieldName]
        {
            get
            {
                Assert.IsTrue(_dict.ContainsKey(fieldName), "Query did not return any column named {0}", fieldName);
                return _dict[fieldName];
            }
        }


        public bool ContainsField(string fieldName)
        {
            return _dict.ContainsKey(fieldName);
        }


        public T GetValue<T>(string fieldName)
        {
            Assert.IsTrue(_dict.ContainsKey(fieldName), "Query did not return any column named {0}", fieldName);
            return (T)_dict[fieldName];
        }

        public T GetValue<T>(int index)
        {
            throw new System.NotImplementedException();
        }
    }
}
