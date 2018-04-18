using System.Web;
using System.Web.Optimization;

namespace Account.Public.Website
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
           

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));


            #region Angular Bundles

            //Core Angular Library
            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                        "~/Scripts/angular.js",
                        "~/Scripts/angular-route.js"
                        ));



            #region Home


            //Main SPA:
            bundles.Add(new Bundle("~/bundles/angular-home").Include(


                        //Main 'App' ------------------------------------------------------
                        "~/angular/home/app.js",

                        //Shared Directives----------------------------------------------------
                        "~/angular/_shared/directives/menuFooter.js",
                        "~/angular/_shared/directives/sectionLoader.js",

                        //Scaffold Index Files ----------------------------------------
                        "~/angular/home/controllers/homeController.js",


                        //Shared Services ---------------------------------------------------------
                        "~/angular/_shared/services/accountServices.js",
                        "~/angular/_shared/services/categoryServices.js"


                        ));


            #endregion

            #region Browse


            //Main SPA:
            bundles.Add(new Bundle("~/bundles/angular-browse").Include(
                        "~/Scripts/jquery-ui.js",
                        "~/Scripts/jquery.nanoscroller.min.js",
                        "~/Scripts/angular-location-update.min.js",
                        //"~/Scripts/angular-masonry.min.js",

                        //Main 'App' & Index File(s)  ----------------------------------------
                        "~/angular/browse/app.js",
                        "~/angular/browse/controllers/browseController.js",

                        //Shared Directives---------------------------------------------------
                        "~/angular/_shared/directives/menuFooter.js",
                        "~/angular/_shared/directives/productItem.js",
                        "~/angular/_shared/directives/salesLeadForm.js",
                        "~/angular/_shared/directives/sectionLoader.js",

                        //Shared Services ----------------------------------------------------
                        "~/angular/_shared/services/accountServices.js",
                        "~/angular/_shared/services/categoryServices.js",
                        "~/angular/_shared/services/searchServices.js",
                        "~/angular/_shared/services/productServices.js",
                        "~/angular/_shared/services/leadServices.js",
                        "~/angular/_shared/services/propertyServices.js"

                        ));


            #endregion


            #region Tablets


            //Main SPA:
            bundles.Add(new Bundle("~/bundles/angular-tablets").Include(
                        "~/Scripts/jquery-ui.js",
                        "~/Scripts/jquery.nanoscroller.min.js",
                        "~/Scripts/angular-location-update.min.js",
                        //"~/Scripts/angular-masonry.min.js",

                        //Main 'App' & Index File(s)  ----------------------------------------
                        "~/angular/tablets/app.js",
                        "~/angular/tablets/controllers/tabletsController.js",

                        //Shared Directives---------------------------------------------------
                        "~/angular/_shared/directives/menuFooter.js",
                        "~/angular/_shared/directives/categoryItemTablet.js",
                        "~/angular/_shared/directives/productItemTablet.js",
                        "~/angular/_shared/directives/salesLeadForm.js",
                        "~/angular/_shared/directives/sectionLoader.js",

                        //Shared Services ----------------------------------------------------
                        "~/angular/_shared/services/accountServices.js",
                        "~/angular/_shared/services/categoryServices.js",
                        "~/angular/_shared/services/searchServices.js",
                        "~/angular/_shared/services/productServices.js",
                        "~/angular/_shared/services/leadServices.js",
                        "~/angular/_shared/services/propertyServices.js"

                        ));


            #endregion

            #region Directory


            //Main SPA:
            bundles.Add(new Bundle("~/bundles/angular-directory").Include(

                        //Main 'App' & Index File(s)  ----------------------------------------
                        "~/angular/directory/app.js",
                        "~/angular/directory/controllers/directoryController.js",

                        //Shared Directives---------------------------------------------------
                        "~/angular/_shared/directives/menuFooter.js",
                        "~/angular/_shared/directives/sectionLoader.js",

                        //Shared Services ----------------------------------------------------
                        "~/angular/_shared/services/accountServices.js",
                        "~/angular/_shared/services/categoryServices.js"

                        ));


            #endregion


            #region Search


            //Main SPA:
            bundles.Add(new Bundle("~/bundles/angular-search").Include(
                        "~/Scripts/jquery.nanoscroller.min.js",

                        //Main 'App' & Index File(s)  ----------------------------------------
                        "~/angular/search/app.js",
                        "~/angular/search/controllers/searchController.js",

                        //Shared Directives---------------------------------------------------
                        "~/angular/_shared/directives/menuFooter.js",
                        "~/angular/_shared/directives/sectionLoader.js",
                        "~/angular/_shared/directives/productItem.js",
                        "~/angular/_shared/directives/sectionLoader.js",

                        //Shared Services ----------------------------------------------------
                        "~/angular/_shared/services/accountServices.js",
                        "~/angular/_shared/services/categoryServices.js",
                        "~/angular/_shared/services/searchServices.js",
                        "~/angular/_shared/services/propertyServices.js"

                        ));


            #endregion


            #region Category


            //Main SPA:
            bundles.Add(new Bundle("~/bundles/angular-category").Include(

                        //Main 'App' -----------------------------------------------------
                        "~/angular/category/app.js",

                        //Shared Directives-----------------------------------------------
                        "~/angular/_shared/directives/menuFooter.js",

                        //Scaffold Index Files -------------------------------------------
                        "~/angular/category/controllers/categoryController.js",

                        //Shared Services ------------------------------------------------
                        "~/angular/_shared/services/accountServices.js",
                        "~/angular/_shared/services/categoryServices.js",
                        "~/angular/_shared/services/browseServices.js"

                        ));


            #endregion



            #endregion






            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));



            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/BootstrapXL.css",
                      "~/Content/font-awesome.css",
                      "~/Content/nanoscroller.css", //<-- Scroller for search & menu sidebars
                      "~/Content/site.css"));

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            // We only optimize in Stage/Production
            if (EnvironmentSettings.CurrentEnvironment.Site == "local" || EnvironmentSettings.CurrentEnvironment.Site == "debug" || EnvironmentSettings.CurrentEnvironment.Site == "stage")
            {
                //Keep JS files unminified & unbundled so we can use console logs as well as Angular Breakpoints during development
                BundleTable.EnableOptimizations = false;
            }
            else
            {
                //Debug.Trace (console.logs) wrappers are OFF and all files are bundled and minified:
                BundleTable.EnableOptimizations = true;
            }
        }
    }
}
