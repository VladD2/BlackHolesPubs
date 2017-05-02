using BlackHoles.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHoles.Entities
{
  public class Article
  {
    public int Id { get; set; }

    [Required, StringLength(8/*, MinimumLength=8*/), Display(Name="№ специальности")]
    public string Specialty { get; set; }

    [Required]
    public Issue Issue { get; set; }

    [Required]
    public ApplicationUser Owner { get; set; }

    [Required, StringLength(600), Display(Name="Заголовок")]
    public string RusArtTitles { get; set; }

    [Required, StringLength(60), Display(Name = "Краткое название")]
    public string ShortArtTitles { get; set; }

    [Required, DataType(DataType.MultilineText), StringLength(3000, MinimumLength=50), Display(Name = "Аннотация")]
    public string RusAbstract { get; set; }

    [Required, DataType(DataType.MultilineText), StringLength(255, MinimumLength=3), Display(Name = "Ключевые слова")]
    public string RusKeywords { get; set; }



    [Required, StringLength(400), Display(Name = "Название статьи")]
    public string EnuArtTitles { get; set; }

    [Required, StringLength(3000, MinimumLength = 50), Display(Name = "Аннотация")]
    public string EnuAbstract { get; set; }

    [Required, StringLength(255, MinimumLength = 3), Display(Name = "Ключевые слова")]
    public string EnuKeywords { get; set; }

    [Required, MinLength(1)]
    public List<Author> Authors { get; set; }
  }
}
