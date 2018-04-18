using System.Web;
using System.Web.Optimization;

namespace AccountRegistration
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //Just using Bundle (not ScriptBundle) to avoid Minification Erros in JS)
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            #region Angular Bundles

            //Core Angular Library //Just using Bundle (not ScriptBundle) to avoid Minification Erros in JS)
            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                        "~/Scripts/angular.js",
                        "~/Scripts/angular-route.js"
                        ));


            #region Registration


            //Registration SPA: //Just using Bundle (not ScriptBundle) to avoid Minification Erros in JS)
            bundles.Add(new Bundle("~/bundles/angular-registration").Include(
                        //"~/Scripts/ui-bootstrap-tpls-{version}.js",

                        //Main 'App' ------------------------------------------------------
                        "~/angular/registration/app.js",

                        //Shared ----------------------------------------------------
                        //"~/angular/shared/directives/sectionLoader.js",

                        //Registration Index Files ----------------------------------------
                        "~/angular/registration/controllers/registration.js",
                        "~/angular/registration/services/registrationServices.js",
                        "~/angular/registration/models/registrationModels.js"

                        ));


            #endregion



            #endregion

            //Just using Bundle (not ScriptBundle) to avoid Minification Erros in JS)
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            //Just using Bundle (not ScriptBundle) to avoid Minification Erros in JS)
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            //Just using Bundle (not ScriptBundle) to avoid Minification Erros in JS)
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/site.css"));
        }
    }
}
