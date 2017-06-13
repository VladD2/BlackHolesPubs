using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackHoles.Models
{
  public enum ArticleStatus
  {
    [Display(Name = "Обрабатывается")]
    RequiresVerification = 0,
    [Display(Name = "Содержит ошибки")]
    HasArrors = 1,
    [Display(Name = "Принята")]
    Accepted = 2,
    [Display(Name = "Оплачена")]
    Paid = 3,
    [Display(Name = "Опубликована")]
    Published = 4,
  }
}
