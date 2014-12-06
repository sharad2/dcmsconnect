using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonEntity
{
    public class CartonListViewModel : ICartonListViewModel
    {
       public IList<CartonHeadlineModel> AllCartons { get; set; }

       public int CartonCount
       {
           get
           {

               return AllCartons.Select(p => p.CartonId).Distinct().Count();
           }
       }
       public int? SumPieces
       {
           get
           {

               return AllCartons.Select(p => p.Pieces).Sum();
           }
       }
    }
}