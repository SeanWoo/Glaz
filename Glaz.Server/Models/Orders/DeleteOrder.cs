using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Glaz.Server.Data.Enums;
using Glaz.Server.Entities;

namespace Glaz.Server.Models.Orders
{
    public class DeleteOrder
    {
        public Guid Id { get; set; }
        
        [Display(Name = "Название вашего заказа")]
        public string Label { get; set; }

        [Display(Name = "Пожелания к заказу")]
        public string Comment { get; set; }
        
        [Display(Name = "Картинка-цель для распознавания")]
        public string TargetImagePath { get; set; }
        
        [Display(Name = "Архив из файлами для вывода при распознавании")]
        public string ResponseFilePath { get; set; }

        public DeleteOrder(Order order)
        {
            Id = order.Id;
            Label = order.Label;
            Comment = order.Comment;
            var target = order.Attachments.First(o => o.Type == AttachmentType.Target);
            TargetImagePath = $"/{target.Path}"; // get path from the server root by /
            var response = order.Attachments.First(o => o.Type == AttachmentType.Archive);
            ResponseFilePath = $"/{response.Path}";
        }
    }
}