using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlackHoles.Utils
{
  public static class DisplayExtensions
  {
    public static MvcHtmlString PublicationMonth(this HtmlHelper htmlHelper, int number)
    {
      switch (number)
      {
        case 1: return new MvcHtmlString("феврале");
        case 2: return new MvcHtmlString("апреле");
        case 3: return new MvcHtmlString("июне");
        case 4: return new MvcHtmlString("августе");
        case 5: return new MvcHtmlString("октябре");
        case 6: return new MvcHtmlString("декабре");
        default: throw new ArgumentException($"Неверное значение номера журнала - {number}. Оно должно лежать в пределах от 1 до 6.", nameof(number));
      }
    }
  }
}