using System.Web.Optimization;

namespace AccountAdminSite
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui.min.js"));

            #region Angular Bundles

            //Core Angular Library
            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                        "~/Scripts/angular.js",
                        "~/Scripts/angular-route.js"
                        ));

            #region Dashboard


            //Dashboard SPA:
            bundles.Add(new Bundle("~/bundles/angular-dashboard").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",

                        //Main 'App' ------------------------------------------------------
                        "~/angular/dashboard/app.js",
                        "~/angular/shared/directives/legalFooter.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/services/accountServices.js",
                        "~/angular/inventory/services/categoryServices.js",
                        "~/angular/search/services/searchServices.js",
                        "~/angular/settings/services/tagServices.js",
                        "~/angular/shared/directives/sectionLoader.js",

                        //Dashboard Index Files ----------------------------------------
                        "~/angular/dashboard/controllers/dashboardIndexController.js",
                        "~/angular/dashboard/services/dashboardIndexServices.js",
                        "~/angular/dashboard/models/dashboardIndexModels.js",
                        "~/angular/dashboard/directives/dashboardTestDirective.js",

                        //Dashboard Details Files ----------------------------------------
                        "~/angular/dashboard/controllers/dashboardDetailController.js",
                        "~/angular/dashboard/services/dashboardDetailServices.js",
                        "~/angular/dashboard/models/dashboardDetailModels.js"//,
                        ));


            #endregion



            #region Inventory


            //Inventory SPA:
            bundles.Add(new Bundle("~/bundles/angular-inventory").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",
                        "~/Scripts/sortable.js",
                        "~/Scripts/combodate.js",
                        "~/Scripts/angular-location-update.min.js", //<-- Allows for updating URL without a refresh

                        //Main 'App' ------------------------------------------------------
                        "~/angular/inventory/app.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/services/accountServices.js",
                        "~/angular/shared/directives/sectionLoader.js",
                        "~/angular/inventory/services/categoryServices.js",
                        "~/angular/inventory/models/categoryModels.js",
                        "~/angular/inventory/directives/reorderItemsModal.js",
                        "~/angular/shared/directives/legalFooter.js",

                        //Mapping Files -------------------------------
                        //"~/Scripts/ng-map.min.js",

                        //Inventory/Categories Index Files ----------------------------------------
                        "~/angular/inventory/controllers/inventoryIndexController.js",
                        "~/angular/inventory/services/inventoryIndexServices.js",
                        "~/angular/inventory/models/inventoryIndexModels.js",
                        "~/angular/inventory/directives/createCategoryModal.js",
                        "~/angular/inventory/directives/categoriesPanel.js",
                        //"~/angular/inventory/directives/reorderUpgradeModal.js",

                        //Product Files ----------------------------------------
                        "~/angular/inventory/controllers/productDetailController.js",
                        "~/angular/inventory/services/productServices.js",
                        "~/angular/inventory/directives/createProductModal.js",
                        "~/angular/inventory/directives/editProductTagsModal.js",
                        "~/angular/inventory/directives/moveProductModal.js",
                        "~/angular/inventory/directives/productPropertiesPanel.js",

                        //Product Property Files ----------------------------------------
                        "~/angular/inventory/directives/propertyManagement/editPredefinedPropertiesModal.js",
                        "~/angular/inventory/directives/propertyManagement/editPredefinedAppendablePropertiesModal.js",
                        "~/angular/inventory/directives/propertyManagement/editBasicPropertiesModal.js",
                        "~/angular/inventory/directives/propertyManagement/editParagraphPropertiesModal.js",
                        "~/angular/inventory/directives/propertyManagement/editDatetimePropertiesModal.js",
                        "~/angular/inventory/directives/propertyManagement/editSwatchPropertiesModal.js",
                        "~/angular/inventory/directives/propertyManagement/editLocationPropertiesModal.js",

                        //Shared Services
                        "~/angular/search/services/searchServices.js",
                        "~/angular/shared/services/accountSettingsServices.js",
                        "~/angular/leads/services/leadsServices.js",

                        //Shared Product Files ----------------------------------------
                        "~/angular/inventory/directives/productsPanel.js",

                        //Tags Files ----------------------------------------
                        "~/angular/settings/services/tagServices.js",


                        //Property Files ----------------------------------------
                        "~/angular/settings/services/propertiesServices.js",

                        //Image Formats Files ----------------------------------------
                        "~/angular/shared/services/imageServices.js",
                        "~/angular/shared/directives/sharedImageManagementPanel.js",

                        //Category Detail Files ----------------------------------------
                        "~/angular/inventory/controllers/categoryDetailController.js",

                        //Subcategory Detail Files ----------------------------------------
                        "~/angular/inventory/controllers/subcategoryDetailController.js",

                        //Subsubcategory Detail Files ----------------------------------------
                        "~/angular/inventory/controllers/subsubcategoryDetailController.js",

                        //Subsubsubcategory Detail Files ----------------------------------------
                        "~/angular/inventory/controllers/subsubsubcategoryDetailController.js"


                        ));


            #endregion

            #region Search


            //Search SPA:
            bundles.Add(new Bundle("~/bundles/angular-search").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",
                        "~/Scripts/jquery.nanoscroller.min.js",

                        //Main 'App' ------------------------------------------------------
                        "~/angular/search/app.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/directives/sectionLoader.js",
                        "~/angular/shared/services/accountServices.js",
                        "~/angular/inventory/services/categoryServices.js",
                        "~/angular/inventory/services/productServices.js",
                        "~/angular/shared/directives/legalFooter.js",

                        //Search Service Files ----------------------------------------
                        "~/angular/search/services/searchServices.js",

                        //Search Index Files ----------------------------------------
                        "~/angular/search/controllers/searchIndexController.js"//,

                        ));



            #endregion

            #region Leads

            //Leads SPA:
            bundles.Add(new Bundle("~/bundles/angular-leads").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",

                        //Main 'App' ------------------------------------------------------
                        "~/angular/leads/app.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/directives/sectionLoader.js",
                        "~/angular/shared/services/accountServices.js",
                        "~/angular/shared/services/accountSettingsServices.js",
                        "~/angular/inventory/services/productServices.js",
                        "~/angular/shared/services/imageServices.js",
                        "~/angular/shared/directives/legalFooter.js",

                        //Leads Index Files ----------------------------------------
                        "~/angular/leads/controllers/leadsController.js",
                        "~/angular/leads/services/leadsServices.js"

                        ));


            #endregion

            #region Settings


            //Settings SPA:
            bundles.Add(new Bundle("~/bundles/angular-settings").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",

                        //Main 'App' ------------------------------------------------------
                        "~/angular/settings/app.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/services/accountSettingsServices.js",
                        "~/angular/shared/directives/sectionLoader.js",
                        "~/angular/shared/services/accountServices.js",
                        "~/angular/leads/services/leadsServices.js",
                        "~/angular/shared/directives/legalFooter.js",

                        //Settings Index Files ----------------------------------------
                        "~/angular/settings/controllers/settingsIndexController.js",

                        //Tags Files ----------------------------------------
                        "~/angular/settings/services/tagServices.js",
                         "~/angular/settings/directives/createTagModal.js",
                         //"~/angular/settings/directives/aboutTagsModal.js",
                         "~/angular/settings/directives/tagsPanel.js",
                         "~/angular/settings/directives/editSalesAlertsModal.js",
                         "~/angular/settings/directives/editLeadLabelsModal.js",
                         "~/angular/settings/directives/editLeadButtonCopyModal.js",
                         "~/angular/settings/directives/editLeadDescriptionCopyModal.js",
                         "~/angular/settings/directives/editCustomDomainModal.js",

                        //Property Files ----------------------------------------
                        "~/angular/settings/services/propertiesServices.js",
                        "~/angular/settings/directives/generalPanel.js",
                        "~/angular/settings/directives/salesPanel.js",
                        "~/angular/settings/directives/propertiesPanel.js",
                        "~/angular/settings/directives/createPropertyModal.js",
                        //"~/angular/settings/directives/aboutPropertiesModal.js",
                        "~/angular/settings/directives/propertyDetailsModal.js",
                        "~/angular/settings/directives/updateFeaturedPropertiesModal.js",

                        //Image Formats Files ----------------------------------------
                        "~/angular/settings/services/imageFormatServices.js",
                        "~/angular/settings/directives/imageFormatsPanel.js"
                        //"~/angular/settings/directives/aboutImageFormatsModal.js"

                        ));


            #endregion

            #region Account


            //Settings SPA:
            bundles.Add(new Bundle("~/bundles/angular-account").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",
                        "~/Scripts/sortable.js",

                        //Main 'App' ------------------------------------------------------
                        "~/angular/account/app.js",
                        "~/angular/shared/directives/legalFooter.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/directives/sectionLoader.js",
                        "~/angular/shared/services/accountServices.js",
                        "~/angular/shared/services/billingServices.js",
                        "~/angular/shared/directives/addUpdateCardModal.js",
                        "~/angular/account/directives/addUpdateContactModal.js",                                            
                        "~/angular/shared/services/accountSettingsServices.js",

                        //account Index Files ----------------------------------------
                        "~/angular/account/controllers/accountIndexController.js",                        
                        "~/angular/account/services/paymentPlanServices.js",
                        "~/angular/account/models/accountIndexModels.js",
                        //"~/angular/account/directives/createSubscriptionModal.js",
                        //"~/angular/account/directives/managePlansModal.js",

                         //Billing Directives ----------------------------------------
                        "~/angular/account/directives/accountBillingDunningAttemptsPanel.js",
                        "~/angular/account/directives/accountBillingInvoiceNextPanel.js",
                        "~/angular/account/directives/accountBillingPaymentHistoryPanel.js",

                        //Image Formats Files ----------------------------------------
                        "~/angular/shared/services/imageServices.js",
                        "~/angular/shared/directives/sharedImageManagementPanel.js",

                        //account User Files ----------------------------------------
                        "~/angular/account/services/accountUserServices.js",
                        "~/angular/account/directives/accountUsersPanel.js",
                        "~/angular/account/directives/accountInvitationsPanel.js"//,

                        ));


            #endregion


            #region API


            //API SPA:
            bundles.Add(new Bundle("~/bundles/angular-api").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",

                        //Main 'App' ------------------------------------------------------
                        "~/angular/api/app.js",
                        "~/angular/api/services/apiKeyServices.js",
                        "~/angular/shared/directives/legalFooter.js",


                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/services/accountServices.js",
                        "~/angular/inventory/services/categoryServices.js",
                        "~/angular/search/services/searchServices.js",
                        "~/angular/settings/services/tagServices.js",
                        "~/angular/shared/directives/sectionLoader.js",
                        "~/angular/settings/services/propertiesServices.js",

                        //API Index Files ----------------------------------------
                        "~/angular/api/controllers/apiController.js"


                        ));


            #endregion


            #region Logs


            //Logs SPA:
            bundles.Add(new Bundle("~/bundles/angular-logs").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",

                        //Main 'App' ------------------------------------------------------
                        "~/angular/logs/app.js",
                        "~/angular/shared/directives/legalFooter.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/directives/sectionLoader.js",
                        "~/angular/account/services/accountUserServices.js",

                        //Logs Index Files ----------------------------------------
                        "~/angular/logs/controllers/logsController.js",
                        "~/angular/shared/services/accountServices.js"
                //"~/angular/logs/models/logsIndexModels.js",
                //"~/angular/logs/directives/logsTestDirective.js",

                        ));


            #endregion

            #region Profile


            //Accounts SPA:
            bundles.Add(new Bundle("~/bundles/angular-profile").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",

                        //Main 'App' ------------------------------------------------------
                        "~/angular/profile/app.js",
                        "~/angular/shared/directives/legalFooter.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/directives/sectionLoader.js",

                        //Platform Index Files ----------------------------------------
                        "~/angular/profile/controllers/profileIndexController.js",
                        "~/angular/profile/services/profileIndexServices.js"
                        ));


            #endregion

            #region Marketplace


            //Marketplace SPA:
            bundles.Add(new Bundle("~/bundles/angular-marketplace").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",
                        "~/Scripts/countUp.js",               

                        //Main 'App' ------------------------------------------------------
                        "~/angular/marketplace/app.js",
                         "~/angular/shared/directives/legalFooter.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/services/commerceServices.js",
                        "~/angular/shared/services/accountServices.js",
                        "~/angular/shared/directives/sectionLoader.js",
                        "~/angular/shared/directives/addUpdateCardModal.js",

                        //Marketplace Index Files ----------------------------------------
                        "~/angular/marketplace/controllers/marketplaceIndexController.js",
                        "~/angular/marketplace/models/marketplaceIndexModels.js",
                        "~/angular/marketplace/directives/marketplaceCreditsPanel.js",
                        "~/angular/marketplace/directives/marketplaceCreditsLogPanel.js",
                        "~/angular/marketplace/directives/marketplaceGoodsServicesPanel.js",
                        "~/angular/marketplace/directives/marketplaceCardPanel.js"

                        ));


            #endregion



            #region Subscribe


            //Subscribe SPA:
            bundles.Add(new Bundle("~/bundles/angular-subscribe").Include(

                        //Main 'App' ------------------------------------------------------
                        "~/angular/subscribe/app.js",

                        

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/directives/sectionLoader.js",
                        "~/angular/shared/services/accountServices.js",
                        "~/angular/account/services/paymentPlanServices.js",
                         "~/angular/shared/directives/legalFooter.js",

                        //Subscribe Index Files ----------------------------------------
                        "~/angular/subscribe/controllers/subscribeController.js"

                        ));


            #endregion




            #region DataInjection


            //DataInjection SPA (Does not run for production):
            
            if(EnvironmentSettings.CurrentEnvironment.Site.ToLower() != "production")
            {
                bundles.Add(new Bundle("~/bundles/angular-dataInjection").Include(
                            "~/Scripts/ui-bootstrap-tpls-{version}.js",
                            "~/Scripts/moment.js",
                            "~/Scripts/angular-moment.js",

                            //Main 'App' ------------------------------------------------------
                            "~/angular/dataInjection/app.js",

                            //Shared ----------------------------------------------------
                            "~/angular/shared/services/sharedServices.js",
                            "~/angular/shared/services/accountServices.js",
                            "~/angular/shared/directives/sectionLoader.js",
                            "~/angular/inventory/services/categoryServices.js",
                            "~/angular/inventory/services/inventoryIndexServices.js",
                            "~/angular/inventory/services/productServices.js",
                            "~/angular/settings/services/tagServices.js",
                            "~/angular/settings/services/propertiesServices.js",
                            "~/angular/shared/services/imageServices.js",

                            //DataInjection Index Files ----------------------------------------
                            "~/angular/dataInjection/controllers/dataInjectionIndexController.js",
                            "~/angular/dataInjection/services/dataInjectionServices.js",
                            "~/angular/search/services/searchServices.js",
                            "~/angular/shared/services/accountSettingsServices.js",

                            //DataInjection Details Files ----------------------------------------
                            "~/angular/dataInjection/controllers/dataInjectionDetailController.js",
                            "~/angular/dataInjection/services/dataInjectionDetailServices.js"//,
                            //"~/angular/dataInjection/models/dataInjectionDetailModels.js",
                            ));
            }

            #endregion


            #region DataInjection-ProductBatch


            //DataInjection SPA (Does not run for production):

            if (EnvironmentSettings.CurrentEnvironment.Site.ToLower() != "production")
            {
                bundles.Add(new Bundle("~/bundles/angular-dataInjection-productBatch").Include(
                            "~/Scripts/ui-bootstrap-tpls-{version}.js",
                            "~/Scripts/moment.js",
                            "~/Scripts/angular-moment.js",

                            //Main 'App' ------------------------------------------------------
                            "~/angular/dataInjection/appProductBatch.js",

                            //Shared ----------------------------------------------------
                            "~/angular/shared/services/sharedServices.js",
                            "~/angular/shared/services/accountServices.js",
                            "~/angular/shared/directives/sectionLoader.js",
                            "~/angular/inventory/services/categoryServices.js",
                            "~/angular/inventory/services/inventoryIndexServices.js",
                            "~/angular/inventory/services/productServices.js",
                            "~/angular/settings/services/tagServices.js",
                            "~/angular/settings/services/propertiesServices.js",
                            "~/angular/shared/services/imageServices.js",

                            //DataInjection Index Files ----------------------------------------
                            "~/angular/dataInjection/controllers/dataInjectionProductBatchController.js",
                            "~/angular/dataInjection/services/dataInjectionServices.js",
                            "~/angular/search/services/searchServices.js",
                            "~/angular/shared/services/accountSettingsServices.js",

                            //DataInjection Details Files ----------------------------------------
                            "~/angular/dataInjection/controllers/dataInjectionDetailController.js",
                            "~/angular/dataInjection/services/dataInjectionDetailServices.js"
                            ));
            }

            #endregion



            #region Scaffold


            //Accounts SPA:
            bundles.Add(new Bundle("~/bundles/angular-scaffold").Include(
                        "~/Scripts/ui-bootstrap-tpls-{version}.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/angular-moment.js",

                         
                        //Main 'App' ------------------------------------------------------
                        "~/angular/scaffold/app.js",

                        //Shared ----------------------------------------------------
                        "~/angular/shared/services/sharedServices.js",
                        "~/angular/shared/directives/sectionLoader.js",
                        "~/angular/shared/directives/legalFooter.js",

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
                      "~/Content/bootstrapXL.css",
                      "~/Content/sidebarlayout.css",
                      "~/Content/site.css",
                      "~/Content/notifications.css",
                      "~/Content/branding.css",
                      "~/Content/buttons.css",
                      "~/Content/font-awesome.css",             
                      "~/Content/Aristo.css", //<-- OVERRIDE JQUERY UI THEME WITH ARISTO
                      "~/Content/nanoscroller.css" //<-- Scroller for search sidebar
                  )); 

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
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
