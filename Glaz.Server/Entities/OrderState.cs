using System;
using System.ComponentModel.DataAnnotations;

namespace Glaz.Server.Entities
{
    public sealed class OrderState
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string Name { get; set; }
    }
}