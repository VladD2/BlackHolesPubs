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

    [Required, StringLength(8/*, MinimumLength=8*/), Display(Name="№ специальности")]
    public string Specialty { get; set; }

    [Display(Name = "Создан")]
    public DateTime Created { get; set; }

    [Display(Name = "Изменен")]
    public DateTime Modified { get; set; }

    [ForeignKey("Issue"), Column(Order = 1)]
    public int IssueYear { get; set; }

    [ForeignKey("Issue"), Column(Order = 2)]
    public int IssueNumber { get; set; }

    [Required]
    public Issue Issue { get; set; }

    [ForeignKey("Owner")]
    public string OwnerId { get; set; }

    [Required]
    public ApplicationUser Owner { get; set; }

    [Required, DataType(DataType.MultilineText), StringLength(600), Display(Name="Заголовок")]
    public string RusArtTitles { get; set; }

    [Required, StringLength(50), Display(Name = "Краткое название")]
    public string ShortArtTitles { get; set; }

    [Required, DataType(DataType.MultilineText), StringLength(3000, MinimumLength=50), Display(Name = "Аннотация")]
    public string RusAbstract { get; set; }

    [Required, DataType(DataType.MultilineText), StringLength(255, MinimumLength=3), Display(Name = "Ключевые слова")]
    public string RusKeywords { get; set; }



    [Required, DataType(DataType.MultilineText), StringLength(400), Display(Name = "Название статьи")]
    public string EnuArtTitles { get; set; }

    [Required, DataType(DataType.MultilineText), StringLength(3000, MinimumLength = 50), Display(Name = "Аннотация")]
    public string EnuAbstract { get; set; }

    [Required, DataType(DataType.MultilineText), StringLength(255, MinimumLength = 3), Display(Name = "Ключевые слова")]
    public string EnuKeywords { get; set; }

    [Required, Display(Name = "Автор(ы)")]
    public List<Author> Authors { get; set; }

    public List<Message> Messages { get; set; }

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
  }
}
