using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackHoles.Models
{
  public enum OrganizationKind
  {
    [Display(Name = "не заполнено")]
    None = 0,
    [Display(Name = "работы")]
    Work = 1,
    [Display(Name = "учебы")]
    Study = 2,
  }
}