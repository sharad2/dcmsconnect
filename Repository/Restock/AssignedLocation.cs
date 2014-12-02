namespace DcmsMobile.PieceReplenish.Repository.Restock
{
    /// <summary>
    /// This is a location in IALOC
    /// </summary>
    internal class AssignedLocation
    {
        /// <summary>
        /// Location where carton can restock
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// Restock Aisle of location
        /// </summary>
        public string RestockAisleId { get; set; }

        /// <summary>
        /// Pieces available on location
        /// </summary>
        public int PiecesAtLocation { get; set; }

        /// <summary>
        /// Space available on location.
        /// </summary>
        public int? SpaceAvailable
        {
            get
            {
                if (this.RailCapacity == null)
                {
                    return null;
                }
                var space = this.RailCapacity.Value - this.PiecesAtLocation;
                return space;
            }
        }

        /// <summary>
        /// Area of location
        /// </summary>
        public string IaId { get; set; }

        /// <summary>
        /// Contain Max Capacity of particular Rail.
        /// </summary>
        public int? RailCapacity { get; set; }

        public string BuildingId { get; set; }

        public int AssignedSkuId { get; set; }

        public string AssignedVwhId { get; set; }
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
