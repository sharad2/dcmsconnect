namespace DcmsMobile.PieceReplenish.Repository.Home
{
    public class Pallet
    {
        public string PalletId { get; set; }

        /// <summary>
        /// Total number of cartons on the pallet
        /// </summary>
        public int CartonCount { get; set; }

        /// <summary>
        /// Any area of carton on pallet
        /// </summary>
        public string MaxCartonArea { get; set; }

        public string PriceSeasonCode { get; set; }
    }
}


/*
    $Id: Pallet.cs 17726 2012-07-26 08:19:26Z bkumar $
    $Revision: 17726 $
    $URL: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/Models/Pallet.cs $
    $Header: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/Models/Pallet.cs 17726 2012-07-26 08:19:26Z bkumar $
    $Author: bkumar $
    $Date: 2012-07-26 13:49:26 +0530 (Thu, 26 Jul 2012) $
*/

