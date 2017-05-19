using BlackHoles.DataContexts;
using BlackHoles.Models;
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
      CreateRolesandUsers();
    }

    // In this method we will create default User roles and Admin user for login    
    private void CreateRolesandUsers()
    {
      var context = new IssuesDb();

      var user = context.Users.Where(u => u.UserName == "vc@rsdn.ru").FirstOrDefault();
      if (user == null)
        return;

      var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
      var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

      CreateRole(user, roleManager, UserManager, Constants.AdminRole);
      CreateRole(user, roleManager, UserManager, Constants.EditorRole);
    }

    private void CreateRole(ApplicationUser user, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, object editor)
    {
      throw new NotImplementedException();
    }

    private static void CreateRole(ApplicationUser user, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> UserManager, string roleName)
    {
      if (!roleManager.RoleExists(roleName))
      {
        var role = new IdentityRole() { Name = roleName };
        roleManager.Create(role);
      }
      if (!UserManager.IsInRole(user.Id, roleName))
      {
        var result = UserManager.AddToRole(user.Id, roleName);
      }
    }
  }
}
