using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHoles.Models
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

    [Required(ErrorMessage = Error.NotSet)]
    [Range((int)OrganizationKind.Work, (int)OrganizationKind.Study, ErrorMessage = "Тип организации должен быть задан!")]
    public OrganizationKind OrganizationKind { get; set; }

    [Required(ErrorMessage = Error.NotSet), StringLength(1000, ErrorMessage = Error.ToLong), Display(Name = "Организация")]
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

    [StringLength(10, ErrorMessage = Error.ToLong), Display(Name = "Индекс")]
    public string Postcode { get; set; }

    [StringLength(300, ErrorMessage = Error.ToLong), Display(Name = "Адрес")]
    public string PostalAddress { get; set; }


    public List<Article> Articles { get; set; }

    public string MakeBriefInitials()
    {
      return string.Concat(this.RusInitials.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x[0] + "."));
    }

    public string MakeBriefFio()
    {
      return this.RusSurname + "\u00A0" + this.MakeBriefInitials();
    }

    public override string ToString()
    {
      return $"{RusSurname} {RusInitials} ({Email})";
    }
  }
}
