            #region Angular Bundles

            //Core Angular Library
            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                        "~/Scripts/angular.js",
                        "~/Scripts/angular-route.js"
                        ));




            #region Scaffold


            //Scaffold SPA:
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

