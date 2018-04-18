(function () {
    'use strict';

    var controllerId = 'dataInjectionIndexController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'sharedServices',
            'dataInjectionServices',
            'productServices',
            'categoryServices',
            'accountServices',
            'tagServices',
            'propertiesServices',
            'imageServices',
             dataInjectionIndexController
    ]);

    function dataInjectionIndexController(sharedServices, dataInjectionServices, productServices, categoryServices, accountServices, tagServices, propertiesServices, imageServices) {

        //Instantiate Controller as ViewModel
        var vm = this;
        vm.account = null;
        vm.maxCategorizationsPerSet = null;
        vm.maxProductsPerSet = null;

        vm.injectionRunning = false;
        //vm.injectionCount = 0;
        vm.outputLog = [];

        vm.injectionSpeed = 3000;

        //Default Properties: =============================
        vm.title = 'dataInjectionIndexController';
        vm.activate = activate;

        /* ==========================================
               Helper Methods
        ==========================================*/

        // Debug Methods ===================================
        // wrap console.log() within the "Debug" namespace
        //    -->  Logs can be removed during minification
        var Debug = {};
        Debug.trace = function (message) {
            console.log(message);
        };


        //Account User:
        vm.currentUserProfile = null;
        var currentUserRoleIndex = null //<-- used internally to check role access, must be updated when getting or refreshing the user.
        var userRoles = []; //<-- used internally to check role access, must be updated when getting or refreshing the user.
        //--------------------------------------------------------------------

        /* ==========================================
             Core Service Properties
        ==========================================*/

        // vm.TrialHoldDays = null; //<----comes from CoreServices (via local feed)
        //vm.CustodianFrequencyDescription = null; //<----comes from CoreServices (via local feed)
        //vm.UnverifiedAccountsDaysToHold = null; //<----comes from CoreServices (via local feed)
        //vm.PlatformWorkerFrequencyDescription = null; //<----comes from CoreServices (via local feed)


        /*===========================================
         
                    GET ACCOUNT
         
         ============================================*/

        vm.getAccount = function () {
            accountServices.getAccount()
                    .success(function (data, status, headers, config) {
                        vm.account = data;

                        vm.maxCategorizationsPerSet = vm.account.PaymentPlan.MaxCategorizationsPerSet;
                        vm.maxProductsPerSet = vm.account.PaymentPlan.MaxProductsPerSet;

                    })
                    .error(function (data, status, headers, config) {

                    })
        }


        /* ==========================================

             CATEGORY INJECTION

        ==========================================*/

        vm.injectCategories = function()
        {
            vm.injectionRunning = true;

            var _type = "category";
            var _description = null;
            var _instance = null;
            var _success = null;
            var _error = null;
            var _code = null;

            var i = 0, max = vm.maxCategorizationsPerSet, delay = vm.injectionSpeed, run;

            run = function () {
                

                _description = "Injecting 'Category " + i + "'";
                _instance = null;
                _success = null;
                _error = null;
                _code = null;

                categoryServices.createCategory("Category " + i, true)
                   .success(function (data, status, headers, config) {

                       //Debug.trace(data);
                       _instance = data.RoleInstance;
                       _success = data.isSuccess;
                       _error = data.ErrorMessage;
                       _code = data.ErrorCode;

                       if(data.isSuccess)
                       {
                           //Process image for new category
                           vm.processImage("1234", "Processing image for: Category " + i, data.SuccessMessage, 'category', 'default', 'thumbnail', 20, 20, 420, 420);

                           //create subcategories for new category
                           vm.injectSubcategories(data.SuccessMessage, "category-" + i);
                       }


                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })

                   })
                   .error(function (data, status, headers, config) {

                       _instance = data.RoleInstance;
                       _error = "An error occurred while attempting to use the service...";

                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })
                   })

               
                if (i++ < max) {
                    setTimeout(run, delay);
                }
                else
                {
                    vm.injectionRunning = false;
                }
            }
            run();

            /*
            for (var i = 0; i < 40; i++)
            {

               //Debug.trace("Creating category " + i + " ...");

               categoryServices.createCategory("Category " + i, true)
               .success(function (data, status, headers, config) {
                   vm.outputLog.push({ type: "category", description: "Creating category '" + "Category " + i + "'", instance: data.RoleInstance, success: data.isSuccess, error: data.ErrorMessage, code: data.ErrorCode })
                        if (data.isSuccess) {

                        }
                        else {
                            //if (data.ErrorCode == "Constraint") {
                                //vm.categoryConstraint = true;
                            //}
                        }

                    })
                   .error(function (data, status, headers, config) {

                            vm.newCategory.Results.IsSuccess = false;

                            //vm.clearInvitationForm();

                            vm.newCategory.IsSending = false;
                            vm.newCategory.SendingComplete = true;
                            vm.newCategory.Results.Message = "An error occurred while attempting to use the service...";
                        })
            }*/

        }



        /* ==========================================

             SUBCATEGORY INJECTION

        ==========================================*/

        vm.injectSubcategories = function (categoryId, categoryNameKey) {
            vm.injectionRunning = true;

            var _type = "subcategory";
            var _description = null;
            var _instance = null;
            var _success = null;
            var _error = null;
            var _code = null;

            var i = 0, max = vm.maxCategorizationsPerSet, delay = vm.injectionSpeed, run;

            run = function () {


                _description = "Injecting 'Subcategory " + i + "' into '" + categoryNameKey + "'";
                _instance = null;
                _success = null;
                _error = null;
                _code = null;

                categoryServices.createSubcategory(categoryId, "Subcategory " + i, true)
                   .success(function (data, status, headers, config) {

                       //Debug.trace(data);
                       _instance = data.RoleInstance;
                       _success = data.isSuccess;
                       _error = data.ErrorMessage;
                       _code = data.ErrorCode;

                       if (data.isSuccess) {
                           //Process image for new category
                           vm.processImage("1234", "Processing image for: Subcategory " + i, data.SuccessMessage, 'subcategory', 'default', 'thumbnail', 20, 20, 420, 420);

                           //create subsubcategories for new subcategory
                           vm.injectSubsubcategories(data.SuccessMessage, "subcategory-" + i);
                       }


                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })

                   })
                   .error(function (data, status, headers, config) {

                       _instance = data.RoleInstance;
                       _error = "An error occurred while attempting to use the service...";

                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })
                   })


                if (i++ < max) {
                    setTimeout(run, delay);
                }
                else {
                    vm.injectionRunning = false;
                }
            }
            run();

        }


        /* ==========================================

             SUBSUBCATEGORY INJECTION

        ==========================================*/

        vm.injectSubsubcategories = function (subcategoryId, subcategoryNameKey) {
            vm.injectionRunning = true;

            var _type = "subsubcategory";
            var _description = null;
            var _instance = null;
            var _success = null;
            var _error = null;
            var _code = null;

            var i = 0, max = vm.maxCategorizationsPerSet, delay = vm.injectionSpeed, run;

            run = function () {


                _description = "Injecting 'Subsubcategory " + i + "' into '" + subcategoryNameKey + "'";
                _instance = null;
                _success = null;
                _error = null;
                _code = null;

                categoryServices.createSubsubcategory(subcategoryId, "Subsubcategory " + i, true)
                   .success(function (data, status, headers, config) {

                       //Debug.trace(data);
                       _instance = data.RoleInstance;
                       _success = data.isSuccess;
                       _error = data.ErrorMessage;
                       _code = data.ErrorCode;

                       if (data.isSuccess) {
                           //Process image for new category
                           vm.processImage("123", "Processing image for: Subsubcategory " + i, data.SuccessMessage, 'subsubcategory', 'default', 'thumbnail', 20, 20, 420, 420);

                           //create subsubsubcategories for new subsubcategory
                           vm.injectSubsubsubcategories(data.SuccessMessage, "subsubcategory-" + i);
                       }


                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })

                   })
                   .error(function (data, status, headers, config) {

                       _instance = data.RoleInstance;
                       _error = "An error occurred while attempting to use the service...";

                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })
                   })


                if (i++ < max) {
                    setTimeout(run, delay);
                }
                else {
                    vm.injectionRunning = false;
                }
            }
            run();

        }


        /* ==========================================

             SUBSUBSUBCATEGORY INJECTION

        ==========================================*/

        vm.injectSubsubsubcategories = function (subsubcategoryId, subsubcategoryNameKey) {
            vm.injectionRunning = true;

            var _type = "subsubsubcategory";
            var _description = null;
            var _instance = null;
            var _success = null;
            var _error = null;
            var _code = null;

            var i = 0, max = vm.maxCategorizationsPerSet, delay = vm.injectionSpeed, run;

            run = function () {


                _description = "Injecting 'Subsubcategory " + i + "' into '" + subsubcategoryNameKey + "'";
                _instance = null;
                _success = null;
                _error = null;
                _code = null;

                categoryServices.createSubsubsubcategory(subsubcategoryId, "Subsubsubcategory " + i, true)
                   .success(function (data, status, headers, config) {

                       //Debug.trace(data);
                       _instance = data.RoleInstance;
                       _success = data.isSuccess;
                       _error = data.ErrorMessage;
                       _code = data.ErrorCode;

                       if (data.isSuccess) {
                           //Process image for new category
                           vm.processImage("5678", "Processing image for: Subsubsubcategory " + i, data.SuccessMessage, 'subsubsubcategory', 'default', 'thumbnail', 20, 20, 420, 420);

                       }


                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })

                   })
                   .error(function (data, status, headers, config) {

                       _instance = data.RoleInstance;
                       _error = "An error occurred while attempting to use the service...";

                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })
                   })


                if (i++ < max) {
                    setTimeout(run, delay);
                }
                else {
                    vm.injectionRunning = false;
                }
            }
            run();

        }

        /* ==========================================

             PRODUCT INJECTION

        ==========================================*/


        vm.injectProducts = function()
        {
            //Loop through each subsubsubcategory and inject products to limits
            categoryServices.getCategoryTree(false)
                    .success(function (data, status, headers, config) {

                        //This is also a nice way of refreshing the redis caches:
                        vm.categoryTree = data;


                        //Get random subsubsubcategories (if any exist)
                        for (var i = 0; i < vm.categoryTree.length; i++) {

                            for (var x = 0; x < vm.categoryTree[i].Subcategories.length; x++) {

                                for (var y = 0; y < vm.categoryTree[i].Subcategories[x].Subsubcategories.length; y++) {


                                    for (var s = 0; s < vm.categoryTree[i].Subcategories[x].Subsubcategories[y].Subsubsubcategories.length; s++) {

                                        var locationPath = vm.categoryTree[i].Subcategories[x].Subsubcategories[y].Subsubsubcategories[s].FullyQualifiedName;
                                        vm.injectCategoryProducts(locationPath);

                                    }
                                }
                            }
                        }


                    })
                    .error(function (data, status, headers, config) {
                        //
                    })
        }


        vm.injectCategoryProducts = function (locationPath) {
            vm.injectionRunning = true;

            var _type = "product";
            var _description = null;
            var _instance = null;
            var _success = null;
            var _error = null;
            var _code = null;

            var i = 0, max = vm.maxProductsPerSet, delay = vm.injectionSpeed, run;

            run = function () {


                _description = "Injecting 'Product " + i + "' into '" + locationPath + "'";
                _instance = null;
                _success = null;
                _error = null;
                _code = null;

                productServices.createProduct(locationPath, "Product " + i, true)
                   .success(function (data, status, headers, config) {

                       //Debug.trace(data);
                       _instance = data.RoleInstance;
                       _success = data.isSuccess;
                       _error = data.ErrorMessage;
                       _code = data.ErrorCode;

                       if (data.isSuccess) {

                           //Get random number between 1-10
                           var random = Math.floor(Math.random() * 10) + 1

                           if (random == 2)
                           {
                               //Process images for new product
                               vm.processImage("5678", "Processing thumbnail image for: product " + i, data.SuccessMessage, 'product', 'default', 'thumbnail', 20, 20, 420, 420);
                               vm.processImage("main1", "Processing main image 1 for: Product " + i, data.SuccessMessage, 'product', 'main', 'featured', 97.2972972972973, 376.2162162162162, 758.9189189189187, 1071.4149443561207);
                           }
                           if (random == 4) {
                               //Process images for new product
                               vm.processImage("123", "Processing thumbnail image for: product " + i, data.SuccessMessage, 'product', 'default', 'thumbnail', 20, 20, 420, 420);
                               vm.processImage("main2", "Processing main image 2 for: Product " + i, data.SuccessMessage, 'product', 'main', 'featured', 1.8435703414316114e-13, 1141.6216216216214, 850.0000000000002, 1200.0000000000002);
                           }
                           if (random == 6) {
                               //Process images for new product
                               vm.processImage("1234", "Processing thumbnail image for: product " + i, data.SuccessMessage, 'product', 'default', 'thumbnail', 20, 20, 420, 420);
                               vm.processImage("main3", "Processing main image 3 for: Product " + i, data.SuccessMessage, 'product', 'main', 'featured', 1.8435703414316114e-13, 564.3243243243243, 850.0000000000001, 1200.0000000000002);
                           }
                           if (random == 8) {
                               //Process images for new product
                               vm.processImage("1233", "Processing thumbnail image for: product " + i, data.SuccessMessage, 'product', 'default', 'thumbnail', 20, 20, 420, 420);
                               vm.processImage("main2", "Processing main image 2 for: Product " + i, data.SuccessMessage, 'product', 'main', 'featured', 1.8435703414316114e-13, 564.3243243243243, 850.0000000000001, 1200.0000000000002);
                           }

                       }


                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })

                   })
                   .error(function (data, status, headers, config) {

                       _instance = data.RoleInstance;
                       _error = "An error occurred while attempting to use the service...";

                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })
                   })


                if (i++ < max) {
                    setTimeout(run, delay);
                }
                else {
                    vm.injectionRunning = false;
                }
            }
            run();

        }





        /* ==========================================
           Image Processing
        ==========================================*/

        vm.processImage = function (imageId, description, objectId, objectType, imageGroupNameKey, imageFormatNameKey, top, left, right, bottom) {
            var _type = objectType + "-image";
            var _description = description;
            var _instance = null;
            var _success = null;
            var _error = null;
            var _code = null;

            dataInjectionServices.processImage(imageId, objectType, objectId, imageGroupNameKey, imageFormatNameKey, 'datainjection', 'jpg', 'jpg', top, left, right, bottom)
                   .success(function (data, status, headers, config) {

                       _instance = data.RoleInstance;
                       _success = data.isSuccess;
                       _error = data.ErrorMessage;
                       _code = data.ErrorCode;

                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })

                   })
                   .error(function (data, status, headers, config) {

                       _instance = data.RoleInstance;
                       _error = "An error occurred while attempting to use the service...";

                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })
                   })
        }
        

        /* ==========================================
           CURRENT USER PROFILE
       ==========================================*/

        function updateCurrentUserProfile() {

            //Debug.trace("Refreshing user profile...");

            sharedServices.getCurrentUserProfile()
            .success(function (data, status, headers, config) {

                vm.currentUserProfile = data; //Used to determine what is shown in the view based on user Role.
                currentUserRoleIndex = vm.userRoles.indexOf(data.Role) //<-- use ACCOUNT roles, NOT PLATFORM roles!

                if (vm.currentUserProfile.Id == "" || vm.currentUserProfile == null) {
                    //Log user out if empty
                    window.location.replace("/login");
                }

                //Debug.trace("Profile refreshed!");
                //Debug.trace("Role index = " + currentUserRoleIndex);

            })
                .error(function (data, status, headers, config) {


                })

        }


        /* ==========================================
               CONTROLLER ACTIVATION
           ==========================================*/

        activate();

        function activate(){

            // Injected variables from the view (via CoreServices/PlatformSettings)
            //Platform --------------------------------------------
            //vm.TrialDaysHold = CoreServiceSettings_Custodian_TrialHoldDays;
            //vm.CustodianFrequencyDescription = CoreServiceSettings_Custodian_FrequencyDescription;
            //vm.UnverifiedAccountsDaysToHold = CoreServiceSettings_Custodian_UnverifiedAccountsDaysToHold;
            //vm.PlatformWorkerFrequencyDescription = CoreServiceSettings_PlatformWorker_FrequencyDescription;

            //Account Roles (used for the logged in Account user, to check Roles accesability
            vm.userRoles = JSON.parse(CoreServiceSettings_AccountUsers_RolesList);
            vm.getAccount();


            // Load local profile for the platfor user.
            vm.currentUserProfile = JSON.parse(CurrentUserProfile);
            currentUserRoleIndex = vm.userRoles.indexOf(vm.currentUserProfile.Role) //<-- Role will indicate what editing capabilites are available.

            if (vm.currentUserProfile.Id == "") {
                //Log user out if empty
                window.location.replace("/login");
            }

            //Update user profile info in case of role updates
            updateCurrentUserProfile();
            // Refresh the profile every 45 seconds (if Role is updated, new editing capabilites will light up for the user)
            setInterval(function () { updateCurrentUserProfile() }, 320000);


            //Debug.trace('dataInjectionIndexController activation complete');



            //Bool: Checks if the users role is allowed
            vm.checkRole = function (lowestRoleAllowed) {

                var allowedIndex = vm.userRoles.indexOf(String(lowestRoleAllowed)); //<-- use Account roles, NOT Platform roles!

                //Debug.trace("Lowest role allowed: '" + lowestRoleAllowed + "'");
                //Debug.trace("Comparing: User: '" + currentUserRoleIndex + "' Allowed: '" + allowedIndex + "'");

                if (currentUserRoleIndex >= allowedIndex) {
                    //Debug.trace("Allowed!");
                    return true;
                }
                else {
                    //Debug.trace("Not allowed!");
                    return false;
                }
            }
        }

    }

})();

