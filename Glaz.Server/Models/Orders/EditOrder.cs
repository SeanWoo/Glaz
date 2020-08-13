using System;
using System.ComponentModel.DataAnnotations;
using Glaz.Server.Data.ValidationAttributes;
using Glaz.Server.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Glaz.Server.Models.Orders
{
    public sealed class EditOrder
    {
        [HiddenInput]
        public Guid Id { get; set; }
        
        const int BytesInOneMebiByte = 1_048_576;
        
        [Required(ErrorMessage = "Название заказа обязательное")]
        [Display(Name = "Название вашего заказа")]
        [MinLength(3, ErrorMessage = "Название должно содержать минимум 3 символа")]
        [MaxLength(128, ErrorMessage = "Название не должно быть слишком длинным (не более 128 символов")]
        public string Label { get; set; }

        [Display(Name = "Пожелания к заказу")]
        public string Comment { get; set; }
        
        [Display(Name = "Картинка-цель для распознавания")]
        [AllowedExtensions(".png", ".jpg")]
        [MaxFileSize(2 * BytesInOneMebiByte)]
        public IFormFile TargetImage { get; set; }
        
        [Display(Name = "Архив из файлами для вывода при распознавании")]
        [AllowedExtensions(".rar", ".zip", ".7z", ".tar.gz")]
        public IFormFile ResponseFile { get; set; }

        public EditOrder()
        {
            // Required by Controller
        }
        
        public EditOrder(Order order)
        {
            Id = order.Id;
            Label = order.Label;
            Comment = order.Comment;
        }
    }
}