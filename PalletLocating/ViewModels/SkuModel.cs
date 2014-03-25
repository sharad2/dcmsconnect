using System.ComponentModel.DataAnnotations;
using DcmsMobile.PalletLocating.Models;

namespace DcmsMobile.PalletLocating.ViewModels
{
    public class SkuModel
    {
        public SkuModel() { }

        public SkuModel(Sku sku)
        {
            this.SkuId = sku.SkuId;
            this.Style = sku.Style;
            this.Color = sku.Color;
            this.Dimension = sku.Dimension;
            this.SkuSize = sku.SkuSize;
            this.UpcCode = sku.UpcCode;
            this.Quantity = sku.Quantity;
        }

        [Required]
        public int SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        [Display(Name = "UPC Code")]
        public string UpcCode { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "SKU")]
        public string DisplaySku
        {
            get
            {
                return string.Format("{0},{1},{2},{3}", Style, Color, Dimension, SkuSize);
            }
        }

        //public override string ToString()
        //{
        //    return string.Format(Style + "," + Color + "," + Dimension + "," + SkuSize);
        //}
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