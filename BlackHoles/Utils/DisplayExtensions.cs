using BlackHoles.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BlackHoles.Utils
{
  public static class DisplayExtensions
  {
    public static string GetDisplayName(this Enum enumValue)
    {
      return enumValue.GetType()
                      .GetMember(enumValue.ToString())
                      .First()
                      .GetCustomAttribute<DisplayAttribute>()
                      .GetName();
    }

    public static MvcHtmlString EnsureEndWithDot(this HtmlHelper htmlHelper, string text)
    {
      if (text.LastOrDefault() != '.')
        text += '.';
      return new MvcHtmlString(htmlHelper.Encode(text));
    }

    public static MvcHtmlString MakeAuthorFioAndScienceDegree(this HtmlHelper htmlHelper, string rusSurname, string rusInitials, string scienceDegree)
    {
      const string nbsp = "&nbsp;";
      var scienceDegreeOpt = string.IsNullOrWhiteSpace(scienceDegree) ? null : (", " + htmlHelper.Encode(scienceDegree));
      var html = "<b>" + htmlHelper.Encode(rusSurname) + nbsp + htmlHelper.Encode(rusInitials).Replace(" ", nbsp) + "</b>" + scienceDegreeOpt;
      if (html.LastOrDefault() != '.')
        html += '.';
      return new MvcHtmlString(html);
    }

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

    public static MvcHtmlString MaximumLengthFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
    {
      var attr = GetStringLengthAttribute(html, expression);
      return new MvcHtmlString(attr.MaximumLength.ToString());
    }

    public static MvcHtmlString MinimumLengthFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
    {
      var attr = GetStringLengthAttribute(html, expression);
      return new MvcHtmlString(attr.MinimumLength.ToString());
    }

    private static StringLengthAttribute GetStringLengthAttribute<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
    {
      var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
      var prop = metadata.ContainerType.GetProperty(metadata.PropertyName);
      var attr = (StringLengthAttribute)prop.GetCustomAttributes(typeof(StringLengthAttribute), true).Single();
      return attr;
    }

    public static string Action(this Controller controller, string actionName, string controllerName, object routeValue = null)
    {
      var dict = routeValue == null ? new RouteValueDictionary() : new RouteValueDictionary(routeValue);
      var referrer = controller.Request.UrlReferrer;
      var host = referrer.Host;
      var port = referrer.Port == 80 ? "" : (":" + referrer.Port);
      var protocol = referrer.Scheme;
      var localUrl = controller.Url.Action(actionName, controllerName, dict);
      var result = $"{protocol}://{host}{localUrl}";
      return result;
    }
  } // class
} // namespace