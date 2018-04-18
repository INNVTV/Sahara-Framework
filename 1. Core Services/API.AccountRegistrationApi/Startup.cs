using System.Web.Http;
using Owin;

namespace API.AccountRegistrationApi
{
    public static class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            #region Set up CoreService Settings for current Enviornment

            
            string environment = string.Empty;

            /*
            try
            {
                environment = RoleEnvironment.GetConfigurationSettingValue("Environment");
                Trace.TraceInformation("Sahara.CoreServices.WebHooks entry point called on environment: " + environment);
            }
            catch
            {
                environment = "local";
            }

            // Initialize Environment Settings
            // MUST have 'Environment' setting in CloudService project inside ServiceConfiguration.[env].csfg file
            Sahara.Core.Settings.Startup.Initialize(environment);

            */

            #endregion

            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            appBuilder.UseWebApi(config);
        }
    }
}
