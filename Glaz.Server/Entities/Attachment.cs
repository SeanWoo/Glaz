using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Glaz.Server.Data.Enums;
using Glaz.Server.Entities.ManyToMany;

namespace Glaz.Server.Entities
{
    public sealed class Attachment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public AttachmentType Type { get; set; }

        [Required]
        [MaxLength(256)]
        public string Label { get; set; }

        public string Note { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public VuforiaDetails Details { get; set; }
        public GlazAccount Account { get; set; }
        public ICollection<AttachmentToOrder> AttachmentToOrders { get; set; }
    }
}