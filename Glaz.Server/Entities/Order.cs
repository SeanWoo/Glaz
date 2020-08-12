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
        [Display(Name = "Имя")]
        public string Label { get; set; }

        [Display(Name = "Комментарий")]
        public string Comment { get; set; }

        [Display(Name = "Комментарий модератора")]
        public string ModeratorComment { get; set; }

        [Display(Name = "Статус")]
        public OrderState State { get; set; }


        #region RelationShips

        public GlazAccount Account { get; set; }

        public ICollection<Attachment> Attachments { get; set; }

        #endregion
    }
}