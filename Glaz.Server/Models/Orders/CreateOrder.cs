using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Glaz.Server.Models.Orders
{
    public sealed class CreateOrder
    {
        [Display(Name = "Order's name")]
        [Required]
        [MinLength(3)]
        [MaxLength(128)]
        public string Label { get; set; }

        [Display(Name = "Comment to the order")]
        public string Comment { get; set; }

        [Required]
        [Display(Name = "Trigger image")]
        public IFormFile TargetImage { get; set; }

        [Required]
        [Display(Name = "Response file on the trigger")]
        public IFormFile ResponseFile { get; set; }
    }
}