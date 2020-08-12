using Glaz.Server.Entities;
using Glaz.Server.Models.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Glaz.Server.Models.ManageOrders
{
    public class ModeratorOrder : ClientOrder
    {
        [Display(Name="Заметка")]
        public string ModeratorComment { get; set; }

        public ModeratorOrder(Order order) : base(order) {
            ModeratorComment = order.ModeratorComment;
        }
    }
}
