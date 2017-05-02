﻿using BlackHoles.Models;
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
    public static string GetUserId(this IPrincipal principal)
    {
      return principal.Identity.GetUserId();
    }

    public static ApplicationUser GetApplicationUser(this IPrincipal principal)
    {
      var userId = principal.Identity.GetUserId();
      var user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(userId);

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