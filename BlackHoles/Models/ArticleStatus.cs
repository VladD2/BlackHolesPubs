using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackHoles.Models
{
  public enum ArticleStatus
  {
    [Display(Name = "Ожидает оборкботки")]
    RequiresVerification = 0,

    [Display(Name = "Обрабатывается")]
    AddedToAntiplagiat = 5,

    [Display(Name = "Проверен в Антиплагиат")]
    AntiplagiatReportLoaded = 6,

    [Display(Name = "Содержит ошибки")]
    HasErrors = 10,

    [Display(Name = "Новая версия")]
    NewVersion = 15,

    [Display(Name = "Принята")]
    Accepted = 20,

    [Display(Name = "Принята и оплачена")]
    Paid = 30,

    [Display(Name = "Принята и оплачена доставка")]
    PaidDelivery = 31,

    [Display(Name = "Не оплачена")]
    PublishedButNotPaid = 40,

    [Display(Name = "Опубликована")]
    Published = 50,
  }
}
