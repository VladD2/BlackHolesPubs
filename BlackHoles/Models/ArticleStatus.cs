using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackHoles.Models
{
  public enum ArticleStatus
  {
    [Display(Name = "Ожидает обработки")]
    RequiresVerification = 0,

    [Display(Name = "Обрабатывается")]
    AddedToAntiplagiat = 5,

    [Display(Name = "Проверен в Антиплагиат")]
    AntiplagiatReportLoaded = 6,

    [Display(Name = "Содержит ошибки")]
    HasErrors = 10,

    [Display(Name = "Отклонена")]
    Rejected = 12,

    [Display(Name = "Нет ответа")]
    NoAnswerForALongTime = 13,

    [Display(Name = "Новая версия")]
    NewVersion = 15,

    [Display(Name = "Принята")]
    Accepted = 20,

    [Display(Name = "Принята +")]
    Paid = 30,

    [Display(Name = "Принята + доставка")]
    PaidDelivery = 31,

    [Display(Name = "Не оплачена")]
    PublishedButNotPaid = 40,

    [Display(Name = "Опубликована")]
    Published = 50,

    [Display(Name = "Опубликована и отправлена")]
    PublishedAndDelivered = 60,
  }
}
