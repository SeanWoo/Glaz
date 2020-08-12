using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Glaz.Server.Data.Enums;
using Glaz.Server.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Glaz.Server.Models.Orders
{
    public class ClientOrder
    {
        [HiddenInput]
        public Guid Id { get; set; }

        [Display(Name = "Название")]
        public string Label { get; set; }

        [Display(Name = "Комментарий")]
        public string Comment { get; set; }

        [Display(Name = "Изображение-цель")]
        public string TargetImagePath { get; set; }

        [Display(Name = "Файл заказчика")]
        public string ResponseFilePath { get; set; }

        [Display(Name = "Статус")]
        public string State { get; set; }

        public ClientOrder() { }
        public ClientOrder(Order order)
        {
            Id = order.Id;
            Label = order.Label;
            Comment = order.Comment;
            TargetImagePath = order.Attachments
                .First(a => a.Type == AttachmentType.Target)
                .Path;
            ResponseFilePath = order.Attachments
                .First(a => a.Type == AttachmentType.Archive)
                .Path;
            State = order.State switch
            {
                OrderState.Deleted => "Удален",
                OrderState.Banned => "Забанен",
                OrderState.Verifying => "На проверке",
                OrderState.Created => "Опубликован",
                _ => throw new InvalidEnumArgumentException("Got unexpected Order.State during creating ClientOrder object")
            };
        }
    }
}