using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BlackHoles.Startup))]
namespace BlackHoles
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
