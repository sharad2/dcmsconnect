using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PieceReplenish.ViewModels
{
    public class CartonViewModel : ViewModelBase
    {
        private string _cartonId;
        /// <summary>
        /// Scanned carton ID
        /// </summary>
        [Display(Name="Carton")]
        [ReadOnly(false)]
        public string CartonId {
            get { return _cartonId; }
            set { _cartonId = value != null ? value.ToUpper() : value; }
        }

        [ReadOnly(true)]
        public IEnumerable<CartonModel> CartonList { get; set; }

        [Required]
        [ReadOnly(false)]
        public ContextModel Context { get; set; }

        /// <summary>
        /// on whhich pallet cartons will be put
        /// </summary>
        [Required]
        [Display(Name = "Pallet")]
        [ReadOnly(false)]
        public string PalletId { get; set; }

        /// <summary>
        /// The Restock aisle for which the puller is pulling. This must be posted to to pull the carton
        /// </summary>
        [Display(Name = "Restock Aisle")]
        [DisplayFormat(NullDisplayText = "Any")]
        [ReadOnly(false)]
        [Required]
        public string RestockAisleId { get; set; }

        [Display(Name = "Cartons on Pallet")]
        [ReadOnly(true)]
        public int CountCartonsOnPallet { get; set; }

        [DisplayFormat(NullDisplayText = "N/A")]
        public string PriceSeasonCode { get; set; }

        /// <summary>
        /// True if the user has rights to pull
        /// </summary>
        public bool IsPuller { get; set; }
    }
}


/*
    $Id: CartonViewModel.cs 17727 2012-07-26 08:19:52Z bkumar $
    $Revision: 17727 $
    $URL: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/ViewModels/CartonViewModel.cs $
    $Header: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/ViewModels/CartonViewModel.cs 17727 2012-07-26 08:19:52Z bkumar $
    $Author: bkumar $
    $Date: 2012-07-26 13:49:52 +0530 (Thu, 26 Jul 2012) $
*/
