using Glaz.Server.Entities;
using Glaz.Server.Models.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Glaz.Server.Data.Enums;

namespace Glaz.Server.Models.ManageOrders
{
    public class ModeratorOrder : ClientOrder
    {
        [Display(Name = "Заметка")]
        public string ModeratorComment { get; set; }
        
        [Display(Name = "Загруженные сборки")]
        public IEnumerable<string> BundlePaths { get; set; }

        public ModeratorOrder(Order order) : base(order)
        {
            TargetImagePath = $"/{TargetImagePath}";
            ResponseFilePath = $"/{ResponseFilePath}";
            ModeratorComment = order.ModeratorComment;

            BundlePaths = order.Attachments
                .Where(a => a.Platform != AttachmentPlatform.None)
                .Select(a => $"/{a.Path}")
                .ToArray();
        }
    }
}
