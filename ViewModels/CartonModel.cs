using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PieceReplenish.ViewModels
{
    public class CartonModel
    {
        [Display(Name = "Carton")]
        public string CartonId { get; set; }

        [Display(Name = "Location")]
        public string LocationId { get; set; }

        [Display(Name = "SKU")]
        public SkuModel SkuInCarton { get; set; }

        public int? SkuReplenishmentPriority { get; set; }
    }
}



/*
    $Id: CartonModel.cs 17727 2012-07-26 08:19:52Z bkumar $
    $Revision: 17727 $
    $URL: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/ViewModels/CartonModel.cs $
    $Header: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/ViewModels/CartonModel.cs 17727 2012-07-26 08:19:52Z bkumar $
    $Author: bkumar $
    $Date: 2012-07-26 13:49:52 +0530 (Thu, 26 Jul 2012) $
*/
