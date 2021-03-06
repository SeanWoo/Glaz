using System.ComponentModel.DataAnnotations;
using Glaz.Server.Data.ValidationAttributes;
using Glaz.Server.Entities;
using Microsoft.AspNetCore.Http;

namespace Glaz.Server.Models.Orders
{
    public sealed class CreateOrder
    {
        const int BytesInOneMebiByte = 1_048_576;
        
        [Display(Name = "Название вашего заказа")]
        [Required(ErrorMessage = "Название заказа обязательное")]
        [MinLength(3, ErrorMessage = "Название должно содержать минимум 3 символа")]
        [MaxLength(128, ErrorMessage = "Название не должно быть слишком длинным (не более 128 символов")]
        public string Label { get; set; }

        [Display(Name = "Пожелания к заказу")]
        public string Comment { get; set; }

        [Required(ErrorMessage = "Картинка-цель обязательная")]
        [Display(Name = "Картинка-цель для распознавания")]
        [AllowedExtensions(".png", ".jpg")]
        [MaxFileSize(2 * BytesInOneMebiByte)]
        public IFormFile TargetImage { get; set; }

        [Required(ErrorMessage = "Архив ответа обязательный")]
        [Display(Name = "Архив из файлами для вывода при распознавании")]
        [AllowedExtensions(".rar", ".zip", ".7z", ".tar.gz")]
        public IFormFile ResponseFile { get; set; }

        public CreateOrder()
        {
            // Required by MVC Controller
        }
        
        public CreateOrder(Order order)
        {
            Label = order.Label;
            Comment = order.Comment;
        }
    }
}