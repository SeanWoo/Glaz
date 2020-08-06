using System;
using System.ComponentModel.DataAnnotations;

namespace Glaz.Server.Entities
{
    public sealed class VuforiaDetails
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string TargetId { get; set; }

        [Required]
        public int TargetVersion { get; set; }

        public byte Rating { get; set; }

        public bool IsBlocked { get; set; }
    }
}
