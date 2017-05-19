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
  public class Author
  {
    [Key]
    public int    Id            { get; set; }

    [ForeignKey("Owner")]
    public string OwnerId { get; set; }

    [Required(ErrorMessage = Error.NotSet)]
    public ApplicationUser Owner { get; set; }

    [Required(ErrorMessage = "Адрес электронной почты должен быть заполнен!"), StringLength(100, ErrorMessage = Error.ToLong)]
    public string Email         { get; set; }

    [Required(ErrorMessage = Error.NotSet), StringLength(100, ErrorMessage = Error.ToLong), Display(Name = "Фамилия")]
    public string RusSurname       { get; set; }

    [Required(ErrorMessage = Error.NotSet), StringLength(100, ErrorMessage = Error.ToLong), Display(Name = "Имя Отчество")]
    public string RusInitials { get; set; }

    [Required(ErrorMessage = Error.NotSet), StringLength(1000, ErrorMessage = Error.ToLong), Display(Name = "Место работы/учебы")]
    public string RusOrgName { get; set; }

    [StringLength(100, ErrorMessage = Error.ToLong), Display(Name = "Подразделение")]
    public string RusSubdivision { get; set; }

    [StringLength(255, ErrorMessage = Error.ToLong), Display(Name = "Должность")]
    public string RusPosition { get; set; }

    [Required(ErrorMessage = Error.NotSet), StringLength(100, ErrorMessage = Error.ToLong), Display(Name = "Фамилия")]
    public string EnuSurname { get; set; }

    [Required(ErrorMessage = Error.NotSet), StringLength(100, ErrorMessage = Error.ToLong), Display(Name = "Имя")]
    public string EnuInitials { get; set; }

    [Required(ErrorMessage = Error.NotSet), StringLength(1000, ErrorMessage = Error.ToLong), Display(Name = "Место работы/учебы")]
    public string EnuOrgName { get; set; }

    [StringLength(100, ErrorMessage = Error.ToLong), Display(Name = "Звание/Степень")]
    public string ScienceDegree { get; set; }

    [StringLength(100, ErrorMessage = Error.ToLong), Display(Name = "Телефон")]
    [DataType(DataType.PhoneNumber)]
    public string Phone         { get; set; }

    public List<Article> Articles { get; set; }

    public string MakeBriefFio()
    {
      string MakeBriefInitials(string fullInitials)
      {
        return string.Concat(fullInitials.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x[0] + "."));
      }

      return this.RusSurname + " " + MakeBriefInitials(this.RusInitials);
    }

    public override string ToString()
    {
      return $"{RusSurname} {RusInitials} ({Email})";
    }
  }
}
