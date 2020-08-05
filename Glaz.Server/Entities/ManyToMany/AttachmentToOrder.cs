using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glaz.Server.Entities.ManyToMany
{
    public sealed class AttachmentToOrder
    {
        public Guid AttachmentId { get; set; }
        public Attachment Attachment { get; set; }

        public Guid OrderId { get; set; }
        public Order Order { get; set; }
    }
}