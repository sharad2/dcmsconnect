
namespace DcmsMobile.PalletLocating.Models
{
    public class Sku
    {
        public int SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string UpcCode { get; set; }

        public int Quantity { get; set; }

        public override string ToString()
        {
            return string.Format(Style + "," + Color + "," + Dimension + "," + SkuSize);
        }
    }
}


/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/