using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHoles.Entities
{
  public static class Error
  {
    public const string ToLong = "Длинна текста в поле '{0}' не должна превышать {1} символов. Минимально допустима длинна - {2}.";
    public const string NotSet = "Поле '{0}' должно быть заполнено!";
  }
}
