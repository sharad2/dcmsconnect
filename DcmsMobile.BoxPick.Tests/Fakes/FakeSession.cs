using System.Collections.Generic;
using System.Web;

namespace DcmsMobile.BoxPick.Tests.Fakes
{
    public class FakeSession : HttpSessionStateBase
    {
        private readonly Dictionary<string, object> _dict;
        private readonly bool _expired;

        public FakeSession(bool expired)
        {
            _dict = new Dictionary<string, object>();
            _expired = expired;
        }

        public override object this[string key]
        {
            get
            {
                return _dict.ContainsKey(key) ? _dict[key] : null;
            }
            set
            {
                _dict[key] = value;
            }
        }

        public override void Remove(string name)
        {
            _dict.Remove(name);
        }

        /// <summary>
        /// Just pretend that this is an ongoing session
        /// </summary>
        public override bool IsNewSession
        {
            get
            {
                return _expired;
            }
        }

        public override void Clear()
        {
            _dict.Clear();
        }
    }
}
