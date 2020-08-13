using System.ComponentModel.DataAnnotations;
using System.Linq;
using Glaz.Server.Data.Enums;
using Glaz.Server.Entities;

namespace Glaz.Server.Models.Orders
{
    public class DetailsOrder : DeleteOrder
    {
        private const int TargetIsNotProcessedRating = -1;
        
        [Display(Name = "Качество распознавания картинки-цели")]
        public int TrackingRate { get; set; }

        public DetailsOrder(Order order) : base(order)
        {
            var target = order.Attachments.First(o => o.Type == AttachmentType.Target);
            TrackingRate = target.VuforiaDetails?.Rating ?? TargetIsNotProcessedRating;
        }
    }
}