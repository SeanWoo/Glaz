using System.ComponentModel.DataAnnotations;
using Glaz.Server.Entities;
using Glaz.Server.Models.Orders;

namespace Glaz.Server.Models.ManageOrders
{
    public sealed class DetailsModerOrder : DetailsOrder
    {
        [Display(Name = "Комментарий модератора")]
        public string ModeratorComment { get; set; }
        
        public DetailsModerOrder(Order order) : base(order)
        {
            ModeratorComment = order.ModeratorComment;
        }
    }
}