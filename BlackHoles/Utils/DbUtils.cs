using BlackHoles.DataContexts;
using BlackHoles.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace BlackHoles.Utils
{
  public static class DbUtils
  {
    public static Issue GetActiveIssueOpt(this IssuesDb db)
    {
      return db.Issues.Where(i => i.Active).OrderBy(i => i.Year).ThenBy(i => i.Number).FirstOrDefault();
    }

    public static bool IsYear(string str)
    {
      var year = 0;
      return int.TryParse(str, out year) ? year > 1990 : false;
    }

    public static int AuthorComparison(Author a, Author b)
    {
      var res = a.RusSurname.CompareTo(b.RusSurname);

      return res == 0 ? a.RusInitials.CompareTo(b.RusInitials) : res;
    }

    public static IQueryable<Article> FilterByOwner(this IQueryable<Article> query, IPrincipal user, string role = Constants.EditorRole)
    {
      var userId = user.GetUserId();

      if (!user.IsInRole(role))
        query = query.Where(a => a.OwnerId == userId);

      return query;
    }

    public static IQueryable<Author> FilterByOwner(this IQueryable<Author> query, IPrincipal user, string role = Constants.EditorRole)
    {
      var userId = user.GetUserId();

      if (!user.IsInRole(role))
        query = query.Where(a => a.OwnerId == userId);

      return query;
    }

    public static Message FindMessageOpt(this List<Message> messages, int id)
    {
      foreach (var message in messages)
      {
        if (message.Id == id)
          return message;

        var foundOpt = FindMessageOpt(message.Messages, id);

        if (foundOpt != null)
          return foundOpt;
      }

      return null;
    }

    public static Message FindParentMessageOpt(this List<Message> messages, Message msg, Message parent = null)
    {
      foreach (var message in messages)
      {
        if (message.Id == msg.Id)
          return parent;

        var foundOpt = FindParentMessageOpt(message.Messages, msg, message);

        if (foundOpt != null)
          return foundOpt;
      }

      return null;
    }

    public static int[] ParseToIntArray(this string authorsIds)
    {
      if (string.IsNullOrWhiteSpace(authorsIds))
        return new int[0];

      return authorsIds.Split(',').Select(int.Parse).ToArray();
    }

    public static string GetUserId(this IPrincipal principal)
    {
      return principal.Identity.GetUserId();
    }

    public static ApplicationUser GetApplicationUser(this IPrincipal principal, IssuesDb db)
    {
      var userId = principal.GetUserId();
      var user = db.Users.Find(userId);
      return user;
    }

    public static void ShowErrors(this Controller controller)
    {
      foreach (var value in controller.ModelState.Values)
      {
        foreach (var error in value.Errors)
        {
          System.Diagnostics.Debug.WriteLine(error.ErrorMessage);
        }
      }
    }

    public static void Revalidate(this Controller controller, object model)
    {
      var modelState = controller.ModelState;
      if (!modelState.IsValid)
      {
        modelState.Clear();
        ModelMetadata modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType());
        ModelValidator compositeValidator = ModelValidator.GetModelValidator(modelMetadata, controller.ControllerContext);

        foreach (ModelValidationResult result in compositeValidator.Validate(null))
        {
          modelState.AddModelError(result.MemberName, result.Message);
        }
      }
    }
  }
}
