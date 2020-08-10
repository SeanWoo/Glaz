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

        public VuforiaDetails(string targetId)
        {
            Id = Guid.NewGuid();
            TargetId = targetId;
            Rating = 0;
            IsBlocked = false;
            TargetVersion = -1;
        }

        public VuforiaDetails(TargetRecord record)
        {
            Id = Guid.NewGuid();
            TargetId = record.TargetId;
            Rating = (byte)record.TrackingRating;
            IsBlocked = false;
            TargetVersion = 0;
        }
    }
}
