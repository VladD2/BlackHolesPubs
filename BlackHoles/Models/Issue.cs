using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHoles.Models
{
  public class Issue
  {
    [Key, Column(Order = 1), Display(Name = "Год")]
    public int Year   { get; set; }

    [Key, Column(Order = 2), Display(Name = "№")]
    public int Number { get; set; }

    [Display(Name = "Формируется в данное время")]
    public bool Active { get; set; }
  }
}
