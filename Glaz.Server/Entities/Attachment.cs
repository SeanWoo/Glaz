using System;
using System.ComponentModel.DataAnnotations;
using Glaz.Server.Data.Enums;

namespace Glaz.Server.Entities
{
    public class Attachment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public AttachmentType Type { get; set; }

        public AttachmentType Platform { get; set; }

        [Required]
        [MaxLength(256)]
        public string Label { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public GlazAccount Account { get; set; }

        public Guid OrderId { get; set; }
        public Order Order { get; set; }
    }
}