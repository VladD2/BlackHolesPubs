using BlackHoles.DataContexts;
using BlackHoles.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System;
using System.Reflection;
using System.Resources;
using System.Globalization;
using System.Web;

[assembly: OwinStartupAttribute(typeof(BlackHoles.Startup))]
namespace BlackHoles
{
  public partial class Startup
  {
    static Assembly GetAssemblyByName(string name)
    {
      return AppDomain.CurrentDomain.GetAssemblies().
             SingleOrDefault(assembly => assembly.GetName().Name == name);
    }

    public void Configuration(IAppBuilder app)
    {
      //var resAsm               = Assembly.Load("Microsoft.AspNet.Identity.EntityFramework.resources, Version=2.0.0.0, Culture=ru, PublicKeyToken=31bf3856ad364e35");
      //var asm                  = Assembly.Load("Microsoft.AspNet.Identity.Core, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
      //var t                    = asm.GetType("Microsoft.AspNet.Identity.Resources");
      //var resourceManField     = t.GetField("resourceMan", BindingFlags.Static | BindingFlags.NonPublic);
      //var resourceManagerRu    = new ResourceManager("Microsoft.AspNet.Identity.EntityFramework.IdentityResources.ru.resources", resAsm);
      //var ruCulture            = CultureInfo.CurrentCulture;
      //var resourceCultureField = t.GetField("resourceCulture", BindingFlags.Static | BindingFlags.NonPublic);
      //resourceManField.SetValue(null, resourceManagerRu);
      //resourceCultureField.SetValue(null, ruCulture);

      ConfigureAuth(app);
      CreateRolesandUsers(app);
    }

    // In this method we will create default User roles and Admin user for login    
    private void CreateRolesandUsers(IAppBuilder app)
    {
      var context = new IssuesDb();

      var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
      var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
      
      userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
      {
        AllowOnlyAlphanumericUserNames = false,
        RequireUniqueEmail = true
      };


      var editor1 = GetUser(context, "alxletur@gmail.com");
      if (editor1 != null)
        AddRole(editor1, roleManager, userManager, Constants.EditorRole);

      var editor2 = GetUser(context, "bp2702@yandex.ru");
      if (editor2 != null)
        AddRole(editor2, roleManager, userManager, Constants.EditorRole);

      var editor3 = GetUser(context, "vc@rsdn.ru");
      if (editor3 != null)
      {
        RemoveRole(editor3, roleManager, userManager, Constants.AdminRole);
        AddRole(editor3, roleManager, userManager, Constants.EditorRole);
      }

      var admin = GetUser(context, "vladdq@ya.ru");
      if (admin != null)
      {
        AddRole(admin, roleManager, userManager, Constants.AdminRole);
        AddRole(admin, roleManager, userManager, Constants.EditorRole);
      }
    }

    private static ApplicationUser GetUser(IssuesDb context, string email)
    {
      return context.Users.Where(u => u.Email == email).SingleOrDefault();
    }

    private static void AddRole(ApplicationUser user, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> UserManager, string roleName)
    {
      if (!roleManager.RoleExists(roleName))
      {
        var role = new IdentityRole() { Name = roleName };
        roleManager.Create(role);
      }
      if (!UserManager.IsInRole(user.Id, roleName))
      {
        var result = UserManager.AddToRole(user.Id, roleName);
        if (!result.Succeeded)
        {
          throw new ApplicationException(string.Join(Environment.NewLine, result.Errors));
        }
      }
    }

    private static void RemoveRole(ApplicationUser user, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> UserManager, string roleName)
    {
      if (!roleManager.RoleExists(roleName))
      {
        var role = new IdentityRole() { Name = roleName };
        roleManager.Create(role);
      }
      if (UserManager.IsInRole(user.Id, roleName))
      {
        var result = UserManager.RemoveFromRole(user.Id, roleName);
      }
    }
  }
}
