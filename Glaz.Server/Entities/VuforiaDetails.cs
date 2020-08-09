using System;
using System.ComponentModel.DataAnnotations;
using Glaz.Server.Data.Vuforia.Responses;

namespace Glaz.Server.Entities
{
    public sealed class VuforiaDetails
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string TargetId { get; set; }

        public int TargetVersion { get; set; }

        public byte Rating { get; set; }

        public bool IsBlocked { get; set; }

        public VuforiaDetails()
        {
            // Required by EF
        }

        public VuforiaDetails(TargetRecord record)
        {
            Id = Guid.NewGuid();
            TargetId = record.TargetId;
            Rating = record.TrackingRating;
            IsBlocked = false;
            TargetVersion = 0;
        }
    }
}
