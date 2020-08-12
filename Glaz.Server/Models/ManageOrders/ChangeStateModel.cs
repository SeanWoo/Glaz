using Glaz.Server.Data.Enums;
using System;

namespace Glaz.Server.Models.ManageOrders
{
    public sealed class ChangeStateModel
    {
        public Guid Id { get; set; }

        public OrderState State { get; set; }

        public string StringState 
        { 
            get => State.GetDisplayName(); 
            set => State = Enum.Parse<OrderState>(value); 
        }

        public ChangeStateModel() { }
        public ChangeStateModel(Guid id, OrderState state)
        {
            Id = id;
            State = state;
        }
    }
}
