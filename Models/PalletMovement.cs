using System;

namespace DcmsMobile.PalletLocating.Models
{
    public class PalletMovement
    {
        public string UserName { get; set; }

        public string Pallet { get; set; }

        public int CountCarton { get; set; }

        public string FromArea { get; set; }

        public string ToArea { get; set; }

        public string FromLocation { get; set; }

        public string ToLocation { get; set; }

        public DateTime? InsertDate { get; set; }
    }
}


    //$Id: PalletInfo.cs 11195 2012-01-02 12:00:24Z spandey $ 
    //$Revision: 11195 $
    //$URL: svn://vcs/net4/Projects/Mvc/DcmsMobile.PalletLocating/trunk/PalletLocating/Models/PalletInfo.cs $
    //$Header: svn://vcs/net4/Projects/Mvc/DcmsMobile.PalletLocating/trunk/PalletLocating/Models/PalletInfo.cs 11195 2012-01-02 12:00:24Z spandey $
    //$Author: spandey $
    //$Date: 2012-01-02 17:30:24 +0530 (Mon, 02 Jan 2012) $
