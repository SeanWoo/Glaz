using System;
using System.ComponentModel.DataAnnotations;

namespace Glaz.Server.Data.Enums
{
    public enum OrderState
    {
        [Display(Name = "На проверке")]
        Verifying,
        [Display(Name = "Опубликован")]
        Published,
        [Display(Name = "Забанен")]
        Banned,
        [Display(Name = "Удален")]
        Deleted
    }
}