
using System.Collections.Generic;

namespace DcmsMobile.Inquiry.Areas.Inquiry.Home
{
    public class ChoiceItem
    {
        public string Description { get; set; }

        public string Url { get; set; }

    }

    public class DisambiguateViewModel
    {
        public IList<ChoiceItem> Choices { get; set; }

        public string Scan { get; set; }
    }
}





//$Id$