using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.ViewModels
{
    public class UnroutedPoModel
    {
        
        [Key]
        public string PoId { get; set; }

        [Key]
        public int Iteration { get; set; }

        [Key]
        public string BuildingId { get; set; }

        [Key]
        public string CustomerDcId { get; set; }

        [DisplayFormat(NullDisplayText = "Bucket Not Available")]
        public int? BucketId { get; set; }

        //This property used to 
        public bool HasBucket
        {
            get
            {
                return this.BucketId != null;
            }
        }

        private int _piecesOrdered;

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PiecesOrdered
        {
            get
            {
                return _piecesOrdered;
            }
            set
            {
                _piecesOrdered = value;
            }
        }

        private int _pickedPieces;

        public int PickedPieces
        {
            get
            {
                return _pickedPieces;
            }

            set
            {
                _pickedPieces = value;
            }
        }

        public int? NumberOfBoxes { get; set; }

        [DisplayFormat(NullDisplayText = "Box not created")]
        public string NumberOfBoxesDisplay
        {
            get
            {
                if (this.NumberOfBoxes == 0)
                {
                    return null;
                }
                return string.Format("{0:N0}", this.NumberOfBoxes);
            }
        }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? StartDate { get; set; }

        public DateTime? MinDcCancelDate { get; set; }

        public decimal? Weight { get; set; }

        public decimal? Volume { get; set; }

        public int PercentFull
        {
            get
            {
                if (this.PickedPieces == 0)
                {
                    return 0;
                }
                if (this.PiecesOrdered == 0)
                {
                    return 0;
                }
                var limit = (int)Math.Round((decimal)this.PickedPieces * 100 / (decimal)this.PiecesOrdered);
                return limit;
            }
        }

        private string _key;
        public string Key
        {
            get
            {
                if (_key == null)
                {
                    var tokens = new[] {
                        this.PoId, this.Iteration.ToString(), this.BuildingId,this.CustomerDcId
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
                this.BuildingId = tokens[2];
                this.CustomerDcId = tokens[3];
            }
        }

        /// <summary>
        /// This property used to highlight the POs having multiple iteration.
        /// </summary>
        public int PoIterationCount { get; set; }

        /// <summary>
        /// Highlight PO if it exist in multiple buildings for given values of PO,Customer DC and Iteration
        /// </summary>
        public int? BuildingCount { get; set; }

        private ICollection<string> _unroutedPoAlertMessages;
        public ICollection<string> UnroutedPoAlertMessages
        {
            get
            {
                if (_unroutedPoAlertMessages != null)
                {
                    return _unroutedPoAlertMessages;
                }
                _unroutedPoAlertMessages = new List<string>(8);
                if (PoIterationCount > 1)
                {
                    _unroutedPoAlertMessages.Add(string.Format("This PO appears {0} times in the list because it was downloaded multiple times from the Vision system", this.PoIterationCount));
                }
                if (BuildingCount > 1)
                {
                    _unroutedPoAlertMessages.Add(string.Format("This PO with same DC exist in another building as well.While routing this PO other one would be routed also."));
                }
                return _unroutedPoAlertMessages;
            }
        }

        public bool IsEdiCustomer { get; set; }
    }
}