using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PieceReplenish.ViewModels
{
    /// <summary>
    /// Base class of all view models
    /// </summary>
    public abstract class ViewModelBase
    {
        /// <summary>
        /// Sound file to play on page load.
        /// </summary>
        public char Sound { get; set; }

        public bool IsEditable { get; set; }

        public string EditableRoleName { get; set; }
    }

    /// <summary>
    /// The context which needs to be passed between pages
    /// </summary>
    public class ContextModel
    {
        [Required]
        [Display(Name = "Building")]
        public string BuildingId { get; set; }

        /// <summary>
        /// Destination Area for whom replenishement/pulling is being done 
        /// </summary>
        [Required]
        [Display(Name = "Picking Area")]
        public string PickAreaId { get; set; }

        /// <summary>
        /// From where cartons/sku will be pulled
        /// </summary>
        [Display(Name = "Source Area")]
        [Required]
        public string CartonAreaId { get; set; }

        [Required]
        [Display(Name = "Restocking Area")]
        public string RestockAreaId { get; set; }

        /// <summary>
        /// Pick Area short name
        /// </summary>
        [Display(Name = "Picking Area")]
        public string ShortName { get; set; }

        public string Serialized
        {
            get
            {
                return string.Join(",", this.BuildingId, this.CartonAreaId, this.PickAreaId, this.RestockAreaId,this.ShortName);
            }
            set
            {
                var tokens = value.Split(',');
                this.BuildingId = tokens[0];
                this.CartonAreaId = tokens[1];
                this.PickAreaId = tokens[2];
                this.RestockAreaId = tokens[3];
                this.ShortName = tokens[4];
            }
        }

    }
}



/*
    $Id: ViewModelBase.cs 17727 2012-07-26 08:19:52Z bkumar $ 
    $Revision: 17727 $
    $URL: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/ViewModels/ViewModelBase.cs $
    $Header: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/ViewModels/ViewModelBase.cs 17727 2012-07-26 08:19:52Z bkumar $
    $Author: bkumar $
    $Date: 2012-07-26 13:49:52 +0530 (Thu, 26 Jul 2012) $
*/