using BlackHoles.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHoles.Entities
{
  public class Article
  {
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = Error.NotSet), StringLength(8, ErrorMessage = Error.ToLong), Display(Name="№ специальности")]
    public string Specialty { get; set; }

    [Display(Name = "Создан")]
    public DateTime Created { get; set; }

    [Display(Name = "Изменен")]
    public DateTime Modified { get; set; }

    [ForeignKey("Issue"), Column(Order = 1)]
    public int IssueYear { get; set; }

    [ForeignKey("Issue"), Column(Order = 2)]
    public int IssueNumber { get; set; }

    [Required(ErrorMessage = Error.NotSet)]
    public Issue Issue { get; set; }

    [ForeignKey("Owner")]
    public string OwnerId { get; set; }

    [Required(ErrorMessage = Error.NotSet)]
    public ApplicationUser Owner { get; set; }

    [Required(ErrorMessage = Error.NotSet), DataType(DataType.MultilineText), StringLength(600, ErrorMessage = Error.ToLong), Display(Name="Заголовок")]
    public string RusArtTitles { get; set; }

    [Required(ErrorMessage = Error.NotSet), StringLength(50, MinimumLength = 3, ErrorMessage = Error.ToLong), Display(Name = "Краткое название")]
    [RegularExpression(@"^[^*|:""<>?/\\]+$", ErrorMessage = @"Краткое название статьи не должно содежать символы '^', '*', '|', '\', ':', '""', '<', '>', '?' и '/'!")]
    public string ShortArtTitles { get; set; }

    [Required(ErrorMessage = Error.NotSet), DataType(DataType.MultilineText), StringLength(2000, MinimumLength=50, ErrorMessage = Error.ToLong), Display(Name = "Аннотация")]
    public string RusAbstract { get; set; }

    [Required(ErrorMessage = Error.NotSet), DataType(DataType.MultilineText), StringLength(255, MinimumLength=3, ErrorMessage = Error.ToLong), Display(Name = "Ключевые слова")]
    public string RusKeywords { get; set; }



    [Required(ErrorMessage = Error.NotSet), DataType(DataType.MultilineText), StringLength(400, ErrorMessage = Error.ToLong), Display(Name = "Название статьи")]
    public string EnuArtTitles { get; set; }

    [Required(ErrorMessage = Error.NotSet), DataType(DataType.MultilineText), StringLength(2000, MinimumLength = 50, ErrorMessage = Error.ToLong), Display(Name = "Аннотация")]
    public string EnuAbstract { get; set; }

    [Required(ErrorMessage = Error.NotSet), DataType(DataType.MultilineText), StringLength(255, MinimumLength = 3, ErrorMessage = Error.ToLong), Display(Name = "Ключевые слова")]
    public string EnuKeywords { get; set; }

    [DataType(DataType.MultilineText), StringLength(8000, ErrorMessage = Error.ToLong), Display(Name = "Список литературы")]
    public string References { get; set; }

    [Required(ErrorMessage = Error.NotSet), Display(Name = "Автор(ы)")]
    public List<Author> Authors { get; set; }

    public List<Message> Messages { get; set; }

    [Display(Name = "Статус")]
    public ArticleStatus Status { get; set; }

    /// <summary>Используется для хранения текста сообщения при создании или записи статить.</summary>
    [NotMapped, DataType(DataType.MultilineText)]
    public string CurrentMessageText { get; set; }

    [NotMapped]
    public string AuthorsIds { get; set; }

    [NotMapped]
    public ArticleAuthorsViewModel AuthorsViewModel { get; set; }

    [NotMapped]
    public int ArticleVersions { get; set; }

    [NotMapped]
    public int ReviewTextVersions { get; set; }

    [NotMapped]
    public int ReviewImgVersions { get; set; }

    [NotMapped]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Без подтверждения согласия вы не можете добавить запрос на публикацию статьи!")]
    //[RegularExpression("true", ErrorMessage = "Вы обязаны принять условия, чтобы добавить запрос на публикацию статьи!")]
    public bool Agreed { get; set; }

    public string GetAuthorsBriefFios()
    {
      return string.Join(", ", this.Authors.Select(a => a.MakeBriefFio()));
    }

    public override string ToString()
    {
      var authors = Authors != null ? string.Join(", ", Authors.Select(a => a.RusSurname)) : "";
      return $"Id={Id} {authors}: {ShortArtTitles}";
    }
  }
}
