using System.Collections.Generic;
using System.Linq;

namespace DcmsMobile.DcmsLite.ViewModels.Pick
{
    public class IndexViewModel : ViewModelBase
    {
        private readonly IList<KeyValuePair<string, BucketModel[]>> _list;
        public IndexViewModel()
        {
            _list = new List<KeyValuePair<string, BucketModel[]>>();
        }

        public IList<KeyValuePair<string, BucketModel[]>> BucketListForPrinting
        {
            get
            {
                return _list;
            }
        }

        public string CustomerId { get; set; }

        public string VwhId { private get; set; }

        public int? SelectedTab
        {
            get
            {
                var result = BucketListForPrinting.Select((v, i) => new {value = v, index = i}).FirstOrDefault(p => p.value.Key == VwhId);
                return result == null ? (int?)null : result.index;
            }
        }
    }
}