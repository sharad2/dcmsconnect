using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DcmsMobile.Receiving.Models;

namespace DcmsMobile.Receiving.ViewModels.Home
{
    public class PalletViewModel
    {

        public int ProcessId { get; set; }

        [Display(Name = "Pallet")]
        public string PalletId { get; set; }

        /// <summary>
        /// The disposition of a pallet is the same as the disposition of the first carton, or null if there are no cartons.
        /// </summary>
        public string DispositionId
        {
            get
            {
                if (this.Cartons.Count == 0)
                {
                    return string.Empty;
                }
                return this.Cartons.First().DispositionId;
            }
        }

        private IList<ReceivedCarton> _cartons;
        public IList<ReceivedCarton> Cartons
        {
            get { return _cartons ?? (_cartons = Enumerable.Empty<ReceivedCarton>().ToList()); }
            set
            {
                _cartons = value;
            }
        }

        /// <summary>
        /// The carton which was last put on the pallet
        /// </summary>
        /// <remarks>
        /// After a carton is received, it becomes the last carton. At all other times this is null.
        /// </remarks>
        // ReSharper disable MemberCanBePrivate.Global
        public string LastCartonId { get; set; }
        // ReSharper restore MemberCanBePrivate.Global

        public int SelectedCartonIndex
        {
            get
            {
                return this.Cartons.Select((p, i) => p.CartonId == this.LastCartonId ? i + 1 : 0).FirstOrDefault(p => p > 0) - 1;
            }
        }

        [Display(Name = "Pallet Limit")]
        public int PalletLimit { get; set; }

        public int QueryCount { get; set; }

      

        public int SkuCount
        {
            get
            {
                return this.Cartons.Select(c => c.Sku.SkuId).Distinct().Count();
            }
        }

        public int PalletProgress
        {
            get
            {
                return this.Cartons.Count * 100 / this.PalletLimit;
            }
        }

        public string StatusMessage { get; set; }

        public string DestinationArea
        {
            get
            {
                if (Cartons == null || Cartons.Count == 0)
                {
                    return string.Empty;
                }
                return Cartons[0].DestinationArea;
            }
        }

        public string VwhId
        {
            get
            {
                if (Cartons == null || Cartons.Count == 0)
                {
                    return string.Empty;
                }
                return Cartons[0].VwhId;
            }
        }
    }
}



//$Id$