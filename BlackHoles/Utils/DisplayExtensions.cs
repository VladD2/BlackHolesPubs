using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace BlackHoles.Utils
{
  public static class BlackHolesExtensions
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
      return PublicationMonth(number);
    }

    public static MvcHtmlString PublicationMonth(int number)
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

    public static string Action(this WebViewPage page, string actionName, string controllerName, object routeValue = null)
    {
      return Action(page.Request, page.Url, actionName, controllerName, routeValue);
    }

    public static string Action(this Controller controller, string actionName, string controllerName, object routeValue = null)
    {
      return Action(controller.Request, controller.Url, actionName, controllerName, routeValue);
    }

    public static string Action(HttpRequestBase request, UrlHelper url, string actionName, string controllerName, object routeValue)
    {
      var dict = routeValue == null ? new RouteValueDictionary() : new RouteValueDictionary(routeValue);
      var uri = request.Url;
      var referrer = request.UrlReferrer ?? uri;
      var host = referrer.Host;
      var port = referrer.Port == 80 ? "" : (":" + referrer.Port);
      var protocol = referrer.Scheme;
      var localUrl = url.Action(actionName, controllerName, dict);
      var result = $"{protocol}://{host}{port}{localUrl}";
      return result;
    }
  } // class
} // namespace