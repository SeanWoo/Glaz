using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Glaz.Server.Models.ManageOrders
{
    public sealed class WriteNoteModel
    {
        [HiddenInput]
        public Guid Id { get; set; }

        [Display(Name = "Оставьте свой комментарий")]
        public string Comment { get; set; }

        public WriteNoteModel() { }
        public WriteNoteModel(Guid id, string comment)
        {
            Id = id;
            Comment = comment;
        }
    }
}
