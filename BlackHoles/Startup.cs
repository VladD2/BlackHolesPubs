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

[assembly: OwinStartupAttribute(typeof(BlackHoles.Startup))]
namespace BlackHoles
{
  public partial class Startup
  {
    public void Configuration(IAppBuilder app)
    {
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
