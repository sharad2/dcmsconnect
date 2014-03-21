using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.DcmsLite.ViewModels.Ship
{
    public class PoModel
    {
        public string PoId { get; set; }

        public string CustomerId { get; set; }

        public string CustomerDcId { get; set; }

        public int? BucketId { get; set; }

        public string CustomerName { get; set; }

        public int? PiecesOrdered { get; set; }

        public int? PickedPieces { get; set; }

        public int? NumberOfBoxes { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? MinDcCancelDate { get; set; }

        public string BuildingId { get; set; }

        public int Iteration { get; set; }

        private string _key;
        public string Key
        {
            get
            {
                if (_key == null)
                {
                    var tokens = new[] {
                        this.PoId, this.Iteration.ToString(), this.CustomerDcId,this.CustomerId
                    };
                    _key = string.Join(",", tokens);
                }
                return _key;
            }
            set
            {
                var tokens = value.Split(',');
                this.PoId = tokens[0];
                this.Iteration = int.Parse(tokens[1]);
                this.CustomerDcId = tokens[2];
                this.CustomerId = tokens[3];
            }
        }
    }
}
