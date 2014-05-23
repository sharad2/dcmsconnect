using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    public class IndexViewModel
    {
        public IList<BuildingModel> Buildings { get; set; }
        
    }
}