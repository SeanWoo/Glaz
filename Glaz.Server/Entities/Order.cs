using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Glaz.Server.Entities.ManyToMany;

namespace Glaz.Server.Entities
{
    public sealed class Order
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Label { get; set; }

        public string Comment { get; set; }

        public string ModeratorComment { get; set; }

        public OrderState State { get; set; }
        public GlazAccount Account { get; set; }
        public ICollection<AttachmentToOrder> AttachmentToOrders { get; set; }
    }
}