
namespace DcmsMobile.BoxManager.Repository
{
    public class CustomerVasSetting
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string VasId { get; set; }

        public string VasPatternDescription { get; set; }

        public string Remark { get; set; }

        public string VasDescription { get; set; }

        public bool InactiveFlag { get; set; }

        public string PatternRegEx { get; set; }
    }
}