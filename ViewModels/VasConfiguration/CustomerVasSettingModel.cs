using System;

namespace DcmsMobile.BoxManager.ViewModels.VasConfiguration
{
    public class CustomerVasSettingModel : IComparable<CustomerVasSettingModel>
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string VasId { get; set; }

        public string VasDescription { get; set; }

        public string VasPatternDescription { get; set; }

        public string Remark { get; set; }

        public bool InactiveFlag { get; set; }

        public int CompareTo(CustomerVasSettingModel other)
        {
            var ret = this.CustomerName.CompareTo(other.CustomerName);
            if (ret == 0 && string.IsNullOrWhiteSpace(VasId))
            {
                ret = this.VasId.CompareTo(other.VasId);
            }
            return ret;
        }
    }
}