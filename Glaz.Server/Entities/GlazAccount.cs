using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Glaz.Server.Entities
{
    public class GlazAccount : IdentityUser
    {
        [MaxLength(128)]
        public string FirstName { get; set; }

        [MaxLength(128)]
        public string LastName { get; set; }

        [Required]
        public DateTime RegisteredAt { get; set; }

        public bool IsBanned { get; set; }

        public bool IsDeleted { get; set; }

        public List<Order> Orders { get; set; }
    }
}