using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Glaz.Server.Models.ManageOrders
{
    public sealed class OrderBundles
    {
        [HiddenInput]
        public Guid OrderId { get; set; }
        
        [Display(Name = "Сборка для Android")]
        public IFormFile AndroidBundle { get; set; }
        
        [Display(Name = "Сборка для IOS")]
        public IFormFile IosBundle { get; set; }
    }
}