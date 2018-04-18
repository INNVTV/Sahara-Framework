using System.Web;
using System.Web.Optimization;

namespace PlatformAdminSite
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //Replaced "ScriptBundle" with "Bundle" due to Minifiacation errors
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));


            #region Angular Bundles

            //Core Angular Library //Replaced "ScriptBundle" with "Bundle" due to Minifiacation errors
            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                        "~/Scripts/angular.js",
                        "~/Scripts/angular-route.js"
                        ));

            #region Dashboard

            //Dashboard: //Replaced "ScriptBundle" with "Bundle" due to Minifiacation errors
            bundles.Add(new ScriptBundle("~/bundles/dashboard").Include(
                        "~/Scripts/moment.js",
                        "~/Scripts/livestamp.js"
                        ));


            #endregion


            #region Accounts


            //Accounts SPA: //Replaced "ScriptBundle" with "Bundle" due to Minifiacation errors
            bundles.Add(new Bundle("~/bundles/angular-accounts").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",
                        "~/Scripts/bootstrap-datepicker.js",
                        "~/Scripts/countUp.js",

                        //Main 'App' ------------------------------------------------------
                        "~/angular/accounts/app.js",
                        
                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/services/commerceServices.js",
                        "~/angular/billing/services/invoiceServices.js",
                        "~/angular/billing/services/billingServices.js",
                        "~/angular/billing/services/paymentServices.js",
                        "~/angular/platform/services/platformServices.js",
                        "~/angular/shared/directives/sectionLoader.js",
                        "~/angular/billing/directives/invoiceHistoryPanel.js",
                        "~/angular/billing/directives/paymentHistoryPanel.js",


                        //Account List Files ----------------------------------------
                        "~/angular/accounts/controllers/accountListController.js",
                        "~/angular/accounts/services/accountListServices.js",
                        "~/angular/billing/services/plansServices.js",
                        "~/angular/accounts/models/accountListModels.js",
                        "~/angular/accounts/directives/accountListGrid.js",
                        "~/angular/accounts/directives/accountListFilter.js",
                        "~/angular/accounts/directives/accountListSearch.js",
                        "~/angular/accounts/directives/accountListMobileSearch.js",
                        "~/angular/accounts/directives/accountDetailApplicationDataPanel.js",
                        "~/angular/accounts/directives/accountDetailDataInjectionPanel.js",

                        //Account Details Files ----------------------------------------
                        "~/angular/accounts/controllers/accountDetailController.js",
                        "~/angular/accounts/services/accountDetailServices.js",
                        "~/angular/accounts/models/accountDetailModels.js",
                        "~/angular/accounts/directives/accountDetailExtendTrial.js",
                        "~/angular/accounts/directives/accountDetailCreateSubscriptionModal.js",
                        "~/angular/accounts/directives/accountDetailAddUpdateCardModal.js",
                        "~/angular/accounts/directives/accountDetailManagePlansModal.js",
                        "~/angular/accounts/directives/accountDetailResetTrial.js",
                        "~/angular/accounts/directives/accountDetailCommunications.js",
                        "~/angular/accounts/directives/accountDetailInvoiceNextPanel.js",
                        "~/angular/accounts/directives/accountDetailDocumentPartitionPanel.js",
                        "~/angular/accounts/directives/accountDetailSqlPartitionPanel.js", 
                        "~/angular/accounts/directives/accountDetailUsersPanel.js",
                        "~/angular/accounts/directives/accountDetailPasswordClaimsPanel.js",   
                        "~/angular/accounts/directives/accountDetailInvitationsPanel.js",
                        "~/angular/accounts/directives/accountDetailDunningAttemptsPanel.js",
                        "~/angular/accounts/directives/commerceCreditsPanel.js",
                        "~/angular/accounts/directives/accountLogsPanel.js"
                        
                        ));

            //Using the "IncludeDirectory" override to merge all .js files under a certain dierctory ()
            //bundles.Add(new ScriptBundle("~/bundles/angular-accounts").IncludeDirectory(
            //"~/Angular/Accounts", "*.js", true));


            #endregion

            #region Platform


            //Accounts SPA: //Replaced "ScriptBundle" with "Bundle" due to Minifiacation errors
            bundles.Add(new Bundle("~/bundles/angular-platform").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",

                        //Main 'App' ------------------------------------------------------
                        "~/angular/platform/app.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/directives/sectionLoader.js",
                        "~/angular/logs/services/logsServices.js",

                        //Platform Index Files ----------------------------------------
                        "~/angular/platform/controllers/platformController.js",
                        "~/angular/platform/services/platformServices.js",
                        "~/angular/billing/services/plansServices.js",
                        "~/angular/platform/models/platformIndexModels.js",
                        "~/angular/platform/directives/platformUsersPanel.js",
                        "~/angular/platform/directives/platformDocumentPartitionsPanel.js",
                        "~/angular/platform/directives/platformSearchPartitionsPanel.js",
                        "~/angular/platform/directives/platformStoragePartitionsPanel.js",
                        "~/angular/platform/directives/platformSqlPartitionsPanel.js",
                        //"~/angular/platform/directives/platformCommunicationsPanel.js", // Removed
                        "~/angular/platform/directives/platformInvitationsPanel.js",
                        "~/angular/platform/directives/platformPasswordClaimsPanel.js"//,
                        //"~/angular/scaffold/directives/scaffoldTestDirective.js",

                        //Scaffold Details Files ----------------------------------------
                        //"~/angular/scaffold/controllers/scaffoldDetailController.js",
                        //"~/angular/scaffold/services/scaffoldDetailServices.js",
                        //"~/angular/scaffold/models/scaffoldDetailModels.js"//,
                        ));


            #endregion

            #region Profile


            //Accounts SPA: //Replaced "ScriptBundle" with "Bundle" due to Minifiacation errors
            bundles.Add(new Bundle("~/bundles/angular-profile").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",

                        //Main 'App' ------------------------------------------------------
                        "~/angular/profile/app.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/directives/sectionLoader.js",

                        //Platform Index Files ----------------------------------------
                        "~/angular/profile/controllers/profileIndexController.js",
                        "~/angular/profile/services/profileIndexServices.js" 
                        ));


            #endregion

            #region Logs


            //Logs SPA: //Replaced "ScriptBundle" with "Bundle" due to Minifiacation errors
            bundles.Add(new Bundle("~/bundles/angular-logs").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",

                        //Main 'App' ------------------------------------------------------
                        "~/angular/logs/app.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/directives/sectionLoader.js",
                        "~/angular/platform/services/platformServices.js",
                        "~/angular/accounts/services/accountDetailServices.js",

                        //Logs Index Files ----------------------------------------
                        "~/angular/logs/controllers/logsController.js",
                        "~/angular/logs/services/logsServices.js"
                        //"~/angular/logs/models/logsIndexModels.js",
                        //"~/angular/logs/directives/logsTestDirective.js",

                        ));


            #endregion

            #region Billing


            //Billing SPA: //Replaced "ScriptBundle" with "Bundle" due to Minifiacation errors
            bundles.Add(new Bundle("~/bundles/angular-billing").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",
                        "~/Scripts/bootstrap-datepicker.js",                       

                        //Main 'App' ------------------------------------------------------
                        "~/angular/billing/app.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/billing/services/invoiceServices.js",
                        "~/angular/billing/services/paymentServices.js",
                        "~/angular/billing/services/transferServices.js",
                        "~/angular/billing/services/billingServices.js",
                        "~/angular/shared/directives/sectionLoader.js",
                        "~/angular/billing/directives/invoiceHistoryPanel.js",
                        "~/angular/billing/directives/paymentHistoryPanel.js",
                        "~/angular/billing/directives/transferHistoryPanel.js",
                        "~/angular/billing/services/plansServices.js",
                        "~/angular/platform/services/platformServices.js",

                        //Billing Index Files ----------------------------------------
                        
                        "~/angular/billing/controllers/billingIndexController.js",                       
                        "~/angular/billing/models/billingIndexModels.js",
                        "~/angular/billing/directives/plansPanel.js",
                        "~/angular/billing/directives/billingReportsPanel.js"
                        

                        //Billing Details Files ----------------------------------------
                        //"~/angular/billing/controllers/billingDetailController.js",
                        //"~/angular/billing/services/billingDetailServices.js",
                        //"~/angular/billing/models/billingDetailModels.js"//,
                        ));


            #endregion

            #region Scaffold


            //Accounts SPA: //Replaced "ScriptBundle" with "Bundle" due to Minifiacation errors
            bundles.Add(new Bundle("~/bundles/angular-scaffold").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",

                        //Main 'App' ------------------------------------------------------
                        "~/angular/scaffold/app.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/directives/sectionLoader.js",

                        //Scaffold Index Files ----------------------------------------
                        "~/angular/scaffold/controllers/scaffoldIndexController.js",
                        "~/angular/scaffold/services/scaffoldIndexServices.js",
                        "~/angular/scaffold/models/scaffoldIndexModels.js",
                        "~/angular/scaffold/directives/scaffoldTestDirective.js",

                        //Scaffold Details Files ----------------------------------------
                        "~/angular/scaffold/controllers/scaffoldDetailController.js",
                        "~/angular/scaffold/services/scaffoldDetailServices.js",
                        "~/angular/scaffold/models/scaffoldDetailModels.js"//,
                        ));


            #endregion


            #endregion

            //Replaced "ScriptBundle" with "Bundle" due to Minifiacation errors
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            //Replaced "ScriptBundle" with "Bundle" due to Minifiacation errors
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            //Replaced "ScriptBundle" with "Bundle" due to Minifiacation errors
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));


            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/sidebarlayout.css",
                      "~/Content/site.css",
                      "~/Content/branding.css",
                      "~/Content/buttons.css",
                      "~/Content/datepicker.css",
                      "~/Content/font-awesome.css",
                      "~/Content/jquery.jqplot.css"));

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            //
            // We only optimize in Stage/Production
            if (EnvironmentSettings.CurrentEnvironment.Site == "local" || EnvironmentSettings.CurrentEnvironment.Site == "debug")
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
