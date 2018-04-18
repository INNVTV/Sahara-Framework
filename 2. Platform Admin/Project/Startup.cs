using Microsoft.Owin;
using Owin;

//Startup class for OWIN and ASP.NET Identity Authentication components

[assembly: OwinStartup(typeof(PlatformAdminSite.Startup))]
namespace PlatformAdminSite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}