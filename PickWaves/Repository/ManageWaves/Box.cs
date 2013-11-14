using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Repository.ManageWaves
{
    public class Box
    {
        [Key]
        public string Ucc128Id { get; set; }

        public string AreaId { get; set; }

        public DateTimeOffset? VerifyDate { get; set; }

        public DateTimeOffset? CancelDate { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public int PickslipId { get; set; }

        public string VWhId { get; set; }

        public string CartonId { get; set; }

        public int? ExpectedPieces { get; set; }

        public int? CurrentPieces { get; set; }

        public DateTimeOffset? PitchingEndDate { get; set; }
    }
}