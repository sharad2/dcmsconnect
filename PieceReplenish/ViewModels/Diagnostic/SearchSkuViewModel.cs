using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.PieceReplenish.ViewModels.Diagnostic
{
    public class SearchSkuViewModel : ViewModelBase
    {
        public SearchSkuViewModel()
        {
            _groupedCartonsToPull = new SortedList<SearchSkuGroup, IList<DiagnosticCartonModel>>(200);
            _groupedCartonsToRestock = new SortedList<SearchSkuGroup, IList<DiagnosticCartonModel>>(200);
            _groupedSkuRequirements = new SortedList<SearchSkuGroup, IList<SkuRequirementModel>>(50);
            AllGroups = new SearchSkuGroup[0];
        }

        [ReadOnly(false)]
        [Required]
        public ContextModel Context { get; set; }

        [Display(Name = "SKU")]
        [Required(ErrorMessage = "Invalid SKU")]
        public int? SkuId { get; set; }

        //For SKU autocomplete
        [Required(ErrorMessage = "Enter an SKU to search for")]
        public string SkuBarCode { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public IList<SearchSkuGroup> AllGroups { get; set; }

        public int ActiveTabIndex
        {
            get
            {
                return AllGroups.Select((p, i) => p.BuildingId == this.Context.BuildingId ? i : -1).FirstOrDefault(p => p >= 0);
            }
        }

        private readonly SortedList<SearchSkuGroup, IList<SkuRequirementModel>> _groupedSkuRequirements;

        public SortedList<SearchSkuGroup, IList<SkuRequirementModel>> GroupedSkuRequirements
        {
            get
            {
                return _groupedSkuRequirements;
            }
        }

        private readonly SortedList<SearchSkuGroup, IList<DiagnosticCartonModel>> _groupedCartonsToPull;

        public SortedList<SearchSkuGroup, IList<DiagnosticCartonModel>> GroupedCartonsToPull
        {
            get
            {
                return _groupedCartonsToPull;
            }
        }

        private readonly SortedList<SearchSkuGroup, IList<DiagnosticCartonModel>> _groupedCartonsToRestock;

        /// <summary>
        /// Cartons in RST
        /// </summary>
        public SortedList<SearchSkuGroup, IList<DiagnosticCartonModel>> GroupedCartonsToRestock
        {
            get
            {
                return _groupedCartonsToRestock;
            }
        }
    }
}