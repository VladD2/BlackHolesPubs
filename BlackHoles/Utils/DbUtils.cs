using BlackHoles.DataContexts;
using BlackHoles.Entities;
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
    public static string MakeBriefFio(this Author author)
    {
      return author.RusSurname + " " + author.RusInitials.MakeBriefInitials();
    }

    public static string MakeBriefInitials(this string fullInitials)
    {
      return string.Concat(fullInitials.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x[0] + "."));
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