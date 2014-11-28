using System;
using System.Collections.Generic;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonAreaEntity
{
    internal class CartonArea 
    {
        public string ShortName { get; set; }

        public string CartonStorageArea { get; set; }

        public string Description { get; set; }

        public bool LocationNumberingFlag { get; set; }

        public bool IsPalletRequired { get; set; }

        public bool OverdraftAllowed { get; set; }

        public bool IsRepackArea { get; set; }

        public string WhID { get; set; }

        
        #region these properties are added to remove use of CartonLocation entity in this model
        public int? TotalLocations { get; set; }

        public int? AssignedLocations { get; set; }

        public int? NonEmptyLocations { get; set; }
        
        #endregion

        public IList<CartonAreaInventory> AllCartons { get; set; }

    }
}





//$Id$