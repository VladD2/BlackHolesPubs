using BlackHoles.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHoles.Entities
{
  public class Author
  {
    public int    Id            { get; set; }

    [Required]
    public ApplicationUser Owner { get; set; }

    [Required, StringLength(100)]
    public string Email         { get; set; }


    [Required, StringLength(100), Display(Name = "Фамилия")]
    public string RusSurname       { get; set; }

    [Required, StringLength(100), Display(Name = "Имя Отчество")]
    public string RusInitials { get; set; }

    [Required, StringLength(1000), Display(Name = "Место работы/учебы")]
    public string RusOrgName { get; set; }

    [StringLength(100), Display(Name = "Подразделение")]
    public string RusSubdivision { get; set; }

    [StringLength(255), Display(Name = "Должность")]
    public string RusPosition { get; set; }

    [Required, StringLength(100), Display(Name = "Фамилия")]
    public string EnuSurname { get; set; }

    [Required, StringLength(100), Display(Name = "Имя")]
    public string EnuInitials { get; set; }

    [Required, StringLength(1000), Display(Name = "Место работы/учебы")]
    public string EnuOrgName { get; set; }

    [StringLength(100), Display(Name = "Звание/Степень")]
    public string ScienceDegree { get; set; }

    [StringLength(100), Display(Name = "Телефон")]
    public string Phone         { get; set; }

    public List<Article> Articles { get; set; }

    public override string ToString()
    {
      return $"{RusSurname} {RusInitials} ({Email})";
    }
  }
}
