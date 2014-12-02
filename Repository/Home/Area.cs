namespace DcmsMobile.PieceReplenish.Repository.Home
{
    public class Area
    {
        public string BuildingId { get; set; }

        public string AreaId { get; set; }

        /// <summary>
        /// Area short name
        /// </summary>
        public string ShortName { get; set; }

        public string Description { get; set; }

        public string CartonAreaId { get; set; }

        public string RestockAreaId { get; set; }

        public int PullableCartonCount { get; set; }
    }
}


/*
    $Id: Area.cs 17726 2012-07-26 08:19:26Z bkumar $
    $Revision: 17726 $
    $URL: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/Models/Area.cs $
    $Header: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/Models/Area.cs 17726 2012-07-26 08:19:26Z bkumar $
    $Author: bkumar $
    $Date: 2012-07-26 13:49:26 +0530 (Thu, 26 Jul 2012) $
*/
