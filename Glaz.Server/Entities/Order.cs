using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        public Guid TargetId { get; set; }
        public Attachment Target { get; set; }

        public Guid ResponseFileId { get; set; }
        public Attachment ResponseFile { get; set; }

        #endregion
    }
}