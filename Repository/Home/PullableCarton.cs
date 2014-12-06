namespace DcmsMobile.PieceReplenish.Repository.Home
{
    /// <summary>
    /// A carton which should be pulled
    /// </summary>
    public class PullableCarton
    {
        public string CartonId { get; set; }

        /// <summary>
        /// Source location of carton
        /// </summary>
        public string LocationId { get; set; }

        public Sku SkuInCarton { get; set; }            

        public int? SkuPriority { get; set; }

        public string RestockAisleId { get; set; }

        public int Quantity { get; set; }

        public string AreaId { get; set; }

        public string VwhId { get; set; }

        public string QualityCode { get; set; }

        public bool IsCartonInSuspense { get; set; }

        public bool IsCartonDamage { get; set; }

        public bool IsWorkNeeded { get; set; }

        public bool IsBestQalityCarton { get; set; }
    }
}


/*
    $Id: Carton.cs 17726 2012-07-26 08:19:26Z bkumar $
    $Revision: 17726 $
    $URL: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/Models/Carton.cs $
    $Header: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/Models/Carton.cs 17726 2012-07-26 08:19:26Z bkumar $
    $Author: bkumar $
    $Date: 2012-07-26 13:49:26 +0530 (Thu, 26 Jul 2012) $
*/
