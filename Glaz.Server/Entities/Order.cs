using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Glaz.Server.Data.Enums;

namespace Glaz.Server.Entities
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Label { get; set; }

        public string Comment { get; set; }

        public string ModeratorComment { get; set; }

        public OrderState State { get; set; }


        #region RelationShips

        public GlazAccount Account { get; set; }
        public VuforiaDetails Details { get; set; }

        public ICollection<Attachment> Attachments { get; set; }

        #endregion
    }
}